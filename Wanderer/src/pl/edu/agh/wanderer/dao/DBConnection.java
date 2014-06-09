package pl.edu.agh.wanderer.dao;

import java.sql.Connection;

import javax.naming.Context;
import javax.naming.InitialContext;
import javax.sql.DataSource;

public class DBConnection {

	private static DataSource postgresDB;
	private static Context context;

	private static DataSource postgresConn() throws Exception {
		if (postgresDB != null) {
			return postgresDB;
		}

		try {
			if (context == null) {
				context = new InitialContext();
			}
			postgresDB = (DataSource) context.lookup("postgis");

		} catch (Exception e) {
			e.printStackTrace();
		}

		return postgresDB;

	}

	protected static Connection getConnection() {
		Connection conn = null;
		try {
			conn = postgresConn().getConnection();
			return conn;
		} catch (Exception e) {
			e.printStackTrace();
		}
		return conn;
	}
}
