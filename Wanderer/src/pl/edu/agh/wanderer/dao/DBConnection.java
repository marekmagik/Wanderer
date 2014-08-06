package pl.edu.agh.wanderer.dao;

import java.sql.Connection;

import javax.naming.Context;
import javax.naming.InitialContext;
import javax.sql.DataSource;

public class DBConnection {

	private static DataSource postgresDB;
	private static Context context;

	/**
	 * Metoda pobierajaca i zwracajca obiekt DataSource reprezentujacy bazê
	 * danych
	 * 
	 * @return obiekt reprezentujacy baze danych
	 * @throws Exception
	 *             w przypadku niepowodzenia przy pobieraniu obiektu
	 */
	private static DataSource postgresConn() throws Exception {
		if (postgresDB != null) {
			return postgresDB;
		}

		try {
			if (context == null) {
				context = new InitialContext();
			}
			postgresDB = (DataSource) context.lookup("postgres");

		} catch (Exception e) {
			e.printStackTrace();
		}

		return postgresDB;

	}

	/**
	 * Metoda zwracajaca polaczenie do bazy danych.
	 * 
	 * @return obiekt Connection reprezentujacy polaczenie z baza
	 */
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
