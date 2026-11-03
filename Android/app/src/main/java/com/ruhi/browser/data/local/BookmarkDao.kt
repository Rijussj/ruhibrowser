package com.ruhi.browser.data.local

import android.content.ContentValues
import android.database.Cursor
import com.ruhi.browser.data.model.Bookmark

interface IBookmarkDao {
    fun insert(bookmark: Bookmark): Long
    fun update(bookmark: Bookmark): Int
    fun delete(id: Long): Int
    fun deleteByUrl(url: String): Int
    fun isBookmarked(url: String): Boolean
    fun getAll(): List<Bookmark>
}

class BookmarkDao(private val dbHelper: RuhiDatabaseHelper) : IBookmarkDao {

    override fun insert(bookmark: Bookmark): Long {
        val db = dbHelper.writableDatabase
        val values = ContentValues().apply {
            put(RuhiDatabaseHelper.COLUMN_BOOKMARK_TITLE, bookmark.title)
            put(RuhiDatabaseHelper.COLUMN_BOOKMARK_URL, bookmark.url)
            put(RuhiDatabaseHelper.COLUMN_BOOKMARK_CREATED_AT, bookmark.createdAt)
            put(RuhiDatabaseHelper.COLUMN_BOOKMARK_POSITION, bookmark.position)
        }
        return db.insert(RuhiDatabaseHelper.TABLE_BOOKMARKS, null, values)
    }

    override fun update(bookmark: Bookmark): Int {
        val db = dbHelper.writableDatabase
        val values = ContentValues().apply {
            put(RuhiDatabaseHelper.COLUMN_BOOKMARK_TITLE, bookmark.title)
            put(RuhiDatabaseHelper.COLUMN_BOOKMARK_URL, bookmark.url)
        }
        return db.update(
            RuhiDatabaseHelper.TABLE_BOOKMARKS,
            values,
            "${RuhiDatabaseHelper.COLUMN_BOOKMARK_ID} = ?",
            arrayOf(bookmark.id.toString())
        )
    }

    override fun delete(id: Long): Int {
        val db = dbHelper.writableDatabase
        return db.delete(
            RuhiDatabaseHelper.TABLE_BOOKMARKS,
            "${RuhiDatabaseHelper.COLUMN_BOOKMARK_ID} = ?",
            arrayOf(id.toString())
        )
    }

    override fun deleteByUrl(url: String): Int {
        val db = dbHelper.writableDatabase
        return db.delete(
            RuhiDatabaseHelper.TABLE_BOOKMARKS,
            "${RuhiDatabaseHelper.COLUMN_BOOKMARK_URL} = ?",
            arrayOf(url)
        )
    }

    override fun isBookmarked(url: String): Boolean {
        val db = dbHelper.readableDatabase
        val cursor = db.query(
            RuhiDatabaseHelper.TABLE_BOOKMARKS,
            arrayOf(RuhiDatabaseHelper.COLUMN_BOOKMARK_ID),
            "${RuhiDatabaseHelper.COLUMN_BOOKMARK_URL} = ?",
            arrayOf(url),
            null,
            null,
            null
        )
        val exists = cursor.moveToFirst()
        cursor.close()
        return exists
    }

    override fun getAll(): List<Bookmark> {
        val list = mutableListOf<Bookmark>()
        val db = dbHelper.readableDatabase
        val cursor = db.query(
            RuhiDatabaseHelper.TABLE_BOOKMARKS,
            null,
            null,
            null,
            null,
            null,
            "${RuhiDatabaseHelper.COLUMN_BOOKMARK_CREATED_AT} DESC"
        )
        while (cursor.moveToNext()) {
            list.add(cursorToBookmark(cursor))
        }
        cursor.close()
        return list
    }

    private fun cursorToBookmark(cursor: Cursor): Bookmark {
        val id = cursor.getLong(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_BOOKMARK_ID))
        val title = cursor.getString(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_BOOKMARK_TITLE))
        val url = cursor.getString(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_BOOKMARK_URL))
        val createdAt = cursor.getLong(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_BOOKMARK_CREATED_AT))
        val position = cursor.getInt(cursor.getColumnIndexOrThrow(RuhiDatabaseHelper.COLUMN_BOOKMARK_POSITION))
        return Bookmark(id, title, url, createdAt, position)
    }
}
