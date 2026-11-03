package com.ruhi.browser.ai

import android.content.Context
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import android.os.Handler
import android.os.Looper
import com.ruhi.browser.data.preferences.SettingsPreferences
import org.json.JSONArray
import org.json.JSONObject
import java.io.BufferedReader
import java.io.InputStreamReader
import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URL
import java.nio.charset.StandardCharsets
import java.util.concurrent.Executors

class AiAssistantClient(private val context: Context) {

    private val preferences = SettingsPreferences(context)
    private val executor = Executors.newSingleThreadExecutor()
    private val mainHandler = Handler(Looper.getMainLooper())

    interface AiResponseListener {
        fun onResponse(response: String)
        fun onError(errorMessage: String)
    }

    fun query(
        userPrompt: String,
        pageContext: String?,
        includePageContext: Boolean,
        listener: AiResponseListener
    ) {
        val apiKey = preferences.aiApiKey.trim()
        if (apiKey.isEmpty()) {
            listener.onError("Hugging Face API token not configured. Please add your token in Settings.")
            return
        }

        if (!isNetworkAvailable()) {
            listener.onError("No internet connection available. Please check your network and retry.")
            return
        }

        executor.execute {
            var connection: HttpURLConnection? = null
            try {
                val endpoint = preferences.aiModelEndpoint.trim().ifEmpty {
                    SettingsPreferences.DEFAULT_AI_ENDPOINT
                }

                val fullPrompt = buildFullPrompt(userPrompt, pageContext, includePageContext)

                val url = URL(endpoint)
                connection = (url.openConnection() as HttpURLConnection).apply {
                    requestMethod = "POST"
                    connectTimeout = 20000
                    readTimeout = 30000
                    doOutput = true
                    doInput = true
                    setRequestProperty("Authorization", "Bearer $apiKey")
                    setRequestProperty("Content-Type", "application/json; charset=UTF-8")
                    setRequestProperty("Accept", "application/json")
                }

                // Construct Request JSON
                val requestPayload = JSONObject().apply {
                    put("inputs", fullPrompt)
                    val params = JSONObject().apply {
                        put("max_new_tokens", 512)
                        put("temperature", 0.7)
                        put("return_full_text", false)
                    }
                    put("parameters", params)
                }

                // Send request
                OutputStreamWriter(connection.outputStream, StandardCharsets.UTF_8).use { writer ->
                    writer.write(requestPayload.toString())
                    writer.flush()
                }

                val responseCode = connection.responseCode
                val inputStream = if (responseCode in 200..299) {
                    connection.inputStream
                } else {
                    connection.errorStream
                }

                val responseBody = BufferedReader(InputStreamReader(inputStream, StandardCharsets.UTF_8)).use { reader ->
                    reader.readText()
                }

                if (responseCode in 200..299) {
                    val generatedText = parseSuccessResponse(responseBody)
                    mainHandler.post {
                        listener.onResponse(generatedText)
                    }
                } else {
                    val errorMessage = parseErrorResponse(responseCode, responseBody)
                    mainHandler.post {
                        listener.onError(errorMessage)
                    }
                }

            } catch (e: Exception) {
                val errorMsg = when (e) {
                    is java.net.SocketTimeoutException -> "Request timed out. The AI model may be loading or busy."
                    is java.net.UnknownHostException -> "Unable to reach the AI endpoint. Check your network or model URL."
                    else -> "AI query failed: ${e.localizedMessage ?: "Unknown error"}"
                }
                mainHandler.post {
                    listener.onError(errorMsg)
                }
            } finally {
                connection?.disconnect()
            }
        }
    }

    private fun buildFullPrompt(
        userPrompt: String,
        pageContext: String?,
        includePageContext: Boolean
    ): String {
        return if (includePageContext && !pageContext.isNullOrBlank()) {
            val truncatedContext = if (pageContext.length > 1500) {
                pageContext.substring(0, 1500) + "…"
            } else {
                pageContext
            }
            "Context from current webpage:\n\"\"\"\n$truncatedContext\n\"\"\"\n\nUser request: $userPrompt"
        } else {
            userPrompt
        }
    }

    private fun parseSuccessResponse(responseBody: String): String {
        try {
            val trimmed = responseBody.trim()
            if (trimmed.startsWith("[")) {
                val array = JSONArray(trimmed)
                if (array.length() > 0) {
                    val first = array.getJSONObject(0)
                    val text = first.optString("generated_text", "")
                    if (text.isNotBlank()) return text.trim()
                }
            } else if (trimmed.startsWith("{")) {
                val obj = JSONObject(trimmed)
                val text = obj.optString("generated_text", "")
                if (text.isNotBlank()) return text.trim()
            }
        } catch (_: Exception) {
            // Fallback to raw text
        }
        return responseBody.trim().ifEmpty { "No response generated." }
    }

    private fun parseErrorResponse(code: Int, responseBody: String): String {
        return when (code) {
            401, 403 -> "Authorization failed (HTTP $code). Verify that your Hugging Face API token is valid."
            429 -> "Rate limit exceeded (HTTP 429). Please wait a moment before sending another request."
            503 -> {
                var estimatedTime = ""
                try {
                    val obj = JSONObject(responseBody)
                    val est = obj.optDouble("estimated_time", 0.0)
                    if (est > 0) estimatedTime = " (Estimated wait: ${est.toInt()}s)"
                } catch (_: Exception) {}
                "Model is currently loading$estimatedTime. Please retry shortly."
            }
            else -> {
                var detail = ""
                try {
                    val obj = JSONObject(responseBody)
                    val err = obj.optString("error", "")
                    if (err.isNotBlank()) detail = ": $err"
                } catch (_: Exception) {}
                "Server error HTTP $code$detail"
            }
        }
    }

    private fun isNetworkAvailable(): Boolean {
        val connectivityManager =
            context.getSystemService(Context.CONNECTIVITY_SERVICE) as? ConnectivityManager ?: return false
        val activeNetwork = connectivityManager.activeNetwork ?: return false
        val caps = connectivityManager.getNetworkCapabilities(activeNetwork) ?: return false
        return caps.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
    }
}
