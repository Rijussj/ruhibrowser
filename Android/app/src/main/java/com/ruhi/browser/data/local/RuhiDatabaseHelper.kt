package com.ruhi.browser.data.local

import android.content.Context
import android.database.sqlite.SQLiteDatabase
import android.database.sqlite.SQLiteOpenHelper

class RuhiDatabaseHelper(context: Context) : SQLiteOpenHelper(
    context.applicationContext,
    DATABASE_NAME,
    null,
    DATABASE_VERSION
) {

    companion object {
        const val DATABASE_NAME = "ruhi_browser.db"
        const val DATABASE_VERSION = 1

        // Bookmarks Table
        const val TABLE_BOOKMARKS = "bookmarks"
        const val COLUMN_BOOKMARK_ID = "id"
        const val COLUMN_BOOKMARK_TITLE = "title"
        const val COLUMN_BOOKMARK_URL = "url"
        const val COLUMN_BOOKMARK_CREATED_AT = "created_at"
        const val COLUMN_BOOKMARK_POSITION = "position"

        // History Table
        const val TABLE_HISTORY = "history"
        const val COLUMN_HISTORY_ID = "id"
        const val COLUMN_HISTORY_TITLE = "title"
        const val COLUMN_HISTORY_URL = "url"
        const val COLUMN_HISTORY_VISITED_AT = "visited_at"
        const val COLUMN_HISTORY_VISIT_COUNT = "visit_count"
    }

    override fun onCreate(db: SQLiteDatabase) {
        val createBookmarksTable = """
            CREATE TABLE $TABLE_BOOKMARKS (
                $COLUMN_BOOKMARK_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                $COLUMN_BOOKMARK_TITLE TEXT NOT NULL,
                $COLUMN_BOOKMARK_URL TEXT NOT NULL,
                $COLUMN_BOOKMARK_CREATED_AT INTEGER NOT NULL,
                $COLUMN_BOOKMARK_POSITION INTEGER DEFAULT 0
            );
        """.trimIndent()

        val createHistoryTable = """
            CREATE TABLE $TABLE_HISTORY (
                $COLUMN_HISTORY_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                $COLUMN_HISTORY_TITLE TEXT NOT NULL,
                $COLUMN_HISTORY_URL TEXT NOT NULL,
                $COLUMN_HISTORY_VISITED_AT INTEGER NOT NULL,
                $COLUMN_HISTORY_VISIT_COUNT INTEGER DEFAULT 1
            );
        """.trimIndent()

        db.execSQL(createBookmarksTable)
        db.execSQL(createHistoryTable)
    }

    override fun onUpgrade(db: SQLiteDatabase, oldVersion: Int, newVersion: Int) {
        // Safe schema migrations when needed
    }
}
