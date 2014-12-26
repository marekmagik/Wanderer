package pl.edu.agh.wanderer.dao;

import java.sql.DriverManager;
import java.sql.Connection;

public class DBConnection {

	/**
	 * Metoda zwracajaca polaczenie do bazy danych.
	 * 
	 * @return obiekt Connection reprezentujacy polaczenie z baza
	 */
	protected static Connection getConnection() {
		Connection conn = null;
		try {
			Class.forName("org.postgresql.Driver");
			conn = DriverManager.getConnection(
					"jdbc:postgresql://127.0.0.1:5432/postgis","postgres", "postgres");
			return conn;
		} catch (Exception e) {
			e.printStackTrace();
		}
		return conn;
	}
}
