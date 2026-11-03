package com.ruhi.browser.data.local

import android.content.ContentValues
import android.database.Cursor
import com.ruhi.browser.data.model.HistoryItem

interface IHistoryDao {
    fun recordVisit(url: String, title: String, visitedAt: Long): Long
    fun getAll(): List<HistoryItem>
    fun delete(id: Long): Int
    fun deleteByUrl(url: String): Int
    fun clearAll(): Int
}

class HistoryDao(private val dbHelper: RuhiDatabaseHelper) : IHistoryDao {

    override fun recordVisit(url: String, title: String, visitedAt: Long): Long {
        val db = dbHelper.writableDatabase

        val cursor = db.query(
            RuhiDatabaseHelper.TABLE_HISTORY,
            arrayOf(RuhiDatabaseHelper.COLUMN_HISTORY_ID, RuhiDatabaseHelper.COLUMN_HISTORY_VISIT_COUNT),
            "${RuhiDatabaseHelper.COLUMN_HISTORY_URL} = ?",
            arrayOf(url),
            null,
            null,
            null
        )

        val id: Long
        if (cursor.moveToFirst()) {
            val existingId = cursor.getLong(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_HISTORY_ID))
            val currentCount = cursor.getInt(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_HISTORY_VISIT_COUNT))
            val values = ContentValues().apply {
                if (title.isNotBlank()) {
                    put(RuhiDatabaseHelper.COLUMN_HISTORY_TITLE, title)
                }
                put(RuhiDatabaseHelper.COLUMN_HISTORY_VISITED_AT, visitedAt)
                put(RuhiDatabaseHelper.COLUMN_HISTORY_VISIT_COUNT, currentCount + 1)
            }
            db.update(
                RuhiDatabaseHelper.TABLE_HISTORY,
                values,
                "${RuhiDatabaseHelper.COLUMN_HISTORY_ID} = ?",
                arrayOf(existingId.toString())
            )
            id = existingId
        } else {
            val values = ContentValues().apply {
                put(RuhiDatabaseHelper.COLUMN_HISTORY_TITLE, if (title.isNotBlank()) title else url)
                put(RuhiDatabaseHelper.COLUMN_HISTORY_URL, url)
                put(RuhiDatabaseHelper.COLUMN_HISTORY_VISITED_AT, visitedAt)
                put(RuhiDatabaseHelper.COLUMN_HISTORY_VISIT_COUNT, 1)
            }
            id = db.insert(RuhiDatabaseHelper.TABLE_HISTORY, null, values)
        }
        cursor.close()
        return id
    }

    override fun getAll(): List<HistoryItem> {
        val list = mutableListOf<HistoryItem>()
        val db = dbHelper.readableDatabase
        val cursor = db.query(
            RuhiDatabaseHelper.TABLE_HISTORY,
            null,
            null,
            null,
            null,
            null,
            "${RuhiDatabaseHelper.COLUMN_HISTORY_VISITED_AT} DESC"
        )
        while (cursor.moveToNext()) {
            list.add(cursorToHistory(cursor))
        }
        cursor.close()
        return list
    }

    override fun delete(id: Long): Int {
        val db = dbHelper.writableDatabase
        return db.delete(
            RuhiDatabaseHelper.TABLE_HISTORY,
            "${RuhiDatabaseHelper.COLUMN_HISTORY_ID} = ?",
            arrayOf(id.toString())
        )
    }

    override fun deleteByUrl(url: String): Int {
        val db = dbHelper.writableDatabase
        return db.delete(
            RuhiDatabaseHelper.TABLE_HISTORY,
            "${RuhiDatabaseHelper.COLUMN_HISTORY_URL} = ?",
            arrayOf(url)
        )
    }

    override fun clearAll(): Int {
        val db = dbHelper.writableDatabase
        return db.delete(RuhiDatabaseHelper.TABLE_HISTORY, null, null)
    }

    private fun cursorToHistory(cursor: Cursor): HistoryItem {
        val id = cursor.getLong(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_HISTORY_ID))
        val title = cursor.getString(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_HISTORY_TITLE))
        val url = cursor.getString(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_HISTORY_URL))
        val visitedAt = cursor.getLong(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_HISTORY_VISITED_AT))
        val visitCount = cursor.getInt(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_HISTORY_VISIT_COUNT))
        return HistoryItem(id, title, url, visitedAt, visitCount)
    }
}
