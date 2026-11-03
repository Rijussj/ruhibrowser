package com.ruhi.browser.util

import android.app.Activity
import android.app.role.RoleManager
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import android.os.Build
import android.provider.Settings
import android.widget.Toast
import androidx.activity.result.ActivityResultLauncher

object DefaultBrowserHelper {

    fun isDefaultBrowser(context: Context): Boolean {
        return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            val roleManager = context.getSystemService(RoleManager::class.java)
            roleManager?.isRoleHeld(RoleManager.ROLE_BROWSER) == true
        } else {
            val intent = Intent(Intent.ACTION_VIEW, Uri.parse("https://www.google.com"))
            val resolveInfo = context.packageManager.resolveActivity(intent, PackageManager.MATCH_DEFAULT_ONLY)
            resolveInfo?.activityInfo?.packageName == context.packageName
        }
    }

    fun requestDefaultBrowser(
        activity: Activity,
        roleLauncher: ActivityResultLauncher<Intent>? = null
    ) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            val roleManager = activity.getSystemService(RoleManager::class.java)
            if (roleManager != null && roleManager.isRoleAvailable(RoleManager.ROLE_BROWSER)) {
                if (roleManager.isRoleHeld(RoleManager.ROLE_BROWSER)) {
                    Toast.makeText(activity, "RUHI Browser is already your default browser", Toast.LENGTH_SHORT).show()
                    return
                }

                val roleIntent = roleManager.createRequestRoleIntent(RoleManager.ROLE_BROWSER)
                if (roleLauncher != null) {
                    try {
                        roleLauncher.launch(roleIntent)
                        return
                    } catch (_: Exception) {}
                } else {
                    try {
                        @Suppress("DEPRECATION")
                        activity.startActivityForResult(roleIntent, 1001)
                        return
                    } catch (_: Exception) {}
                }
            }
        }

        // Multi-level fallback across all OEMs (Samsung, Xiaomi/MIUI, Vivo, Oppo, OnePlus, Stock)
        openDefaultAppsSettings(activity)
    }

    fun openDefaultAppsSettings(context: Context) {
        val intents = listOf(
            Intent(Settings.ACTION_MANAGE_DEFAULT_APPS_SETTINGS),
            Intent("android.settings.MANAGE_DEFAULT_APPS_SETTINGS"),
            Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS).apply {
                data = Uri.parse("package:${context.packageName}")
            },
            Intent(Settings.ACTION_SETTINGS)
        )

        for (intent in intents) {
            try {
                intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK
                context.startActivity(intent)
                Toast.makeText(
                    context,
                    "Select 'Default apps' -> 'Browser app' and choose RUHI Browser",
                    Toast.LENGTH_LONG
                ).show()
                return
            } catch (_: Exception) {}
        }

        Toast.makeText(context, "Please open Android Settings to set default browser", Toast.LENGTH_SHORT).show()
    }
}
