package pl.edu.agh.wanderer.util;

import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.util.ArrayList;
import java.util.List;

import org.codehaus.jettison.json.JSONArray;
import org.codehaus.jettison.json.JSONObject;

/**
 * Klasa odpowiadajaca za konwertowanie wyników zapytania do baza danych na dane
 * w formacie JSON.
 * 
 */
public class JSONConverter {

	/**
	 * Metoda konwertujaca wynik zapytania z bazy danych na dane w formacie JSON
	 * (JSONArray).
	 * 
	 * @param rs
	 *            zbiór wynikowy zapytania
	 * @return obiekt JSONArray reprezentujacy otrzymane dane
	 * @throws Exception
	 *             w przypadku niepowodzenia podczas konwertowania
	 */
	public JSONArray toJSONArray(ResultSet rs) throws Exception {

		JSONArray json = new JSONArray();

		try {

			java.sql.ResultSetMetaData rsmd = rs.getMetaData();

			while (rs.next()) {

				int numColumns = rsmd.getColumnCount();
				JSONObject obj = new JSONObject();

				for (int i = 1; i < numColumns + 1; i++) {

					String column_name = rsmd.getColumnName(i);

					if (rsmd.getColumnType(i) == java.sql.Types.ARRAY) {
						obj.put(column_name, rs.getArray(column_name));
						/* Debug */System.out.println("ToJson: ARRAY");
					} else if (rsmd.getColumnType(i) == java.sql.Types.BIGINT) {
						obj.put(column_name, rs.getInt(column_name));
						/* Debug */System.out.println("ToJson: BIGINT");
					} else if (rsmd.getColumnType(i) == java.sql.Types.BOOLEAN) {
						obj.put(column_name, rs.getBoolean(column_name));
						/* Debug */System.out.println("ToJson: BOOLEAN");
					} else if (rsmd.getColumnType(i) == java.sql.Types.BLOB) {
						obj.put(column_name, rs.getBlob(column_name));
						/* Debug */System.out.println("ToJson: BLOB");
					} else if (rsmd.getColumnType(i) == java.sql.Types.DOUBLE) {
						obj.put(column_name, rs.getDouble(column_name));
						/* Debug */System.out.println("ToJson: DOUBLE");
					} else if (rsmd.getColumnType(i) == java.sql.Types.FLOAT) {
						obj.put(column_name, rs.getFloat(column_name));
						/* Debug */System.out.println("ToJson: FLOAT");
					} else if (rsmd.getColumnType(i) == java.sql.Types.INTEGER) {
						obj.put(column_name, rs.getInt(column_name));
						/* Debug */System.out.println("ToJson: INTEGER");
					} else if (rsmd.getColumnType(i) == java.sql.Types.NVARCHAR) {
						obj.put(column_name, rs.getNString(column_name));
						/* Debug */System.out.println("ToJson: NVARCHAR");
					} else if (rsmd.getColumnType(i) == java.sql.Types.VARCHAR) {
						obj.put(column_name, rs.getString(column_name));
						/* Debug */System.out.println("ToJson: VARCHAR");
					} else if (rsmd.getColumnType(i) == java.sql.Types.TINYINT) {
						obj.put(column_name, rs.getInt(column_name));
						/* Debug */System.out.println("ToJson: TINYINT");
					} else if (rsmd.getColumnType(i) == java.sql.Types.SMALLINT) {
						obj.put(column_name, rs.getInt(column_name));
						/* Debug */System.out.println("ToJson: SMALLINT");
					} else if (rsmd.getColumnType(i) == java.sql.Types.DATE) {
						obj.put(column_name, rs.getDate(column_name));
						/* Debug */System.out.println("ToJson: DATE");
					} else if (rsmd.getColumnType(i) == java.sql.Types.TIMESTAMP) {
						obj.put(column_name, rs.getTimestamp(column_name));
						/* Debug */System.out.println("ToJson: TIMESTAMP");
					} else if (rsmd.getColumnType(i) == java.sql.Types.NUMERIC) {
						obj.put(column_name, rs.getBigDecimal(column_name));
						/* Debug */System.out.println("ToJson: NUMERIC");
					} else {
						obj.put(column_name, rs.getObject(column_name));
						/* Debug */System.out.println("ToJson: Object " + column_name);
					}
				}

				json.put(obj);

			}

		} catch (Exception e) {
			e.printStackTrace();
		}

		return json;
	}

	/**
	 * Metoda konwertujaca liste obiektow JSONObject do stringu w postaci JSON.
	 * 
	 * @param jsonObjects
	 *            lista obiektów JSON
	 * @return string w formacie JSON
	 */
	public String convertListOfJSONObjects(List<JSONObject> jsonObjects) {
		JSONArray jsonArray = new JSONArray();
		for (JSONObject json : jsonObjects) {
			jsonArray.put(json);
		}
		return jsonArray.toString();
	}

	/**
	 * Metoda konwertujaca zbiór wynikowy zapytania do bazy danych na dane w
	 * formacie JSON (List<JSONObject>).
	 * 
	 * @param rs
	 *            zbiór wynikowy zapytania do bazy
	 * @return lista obiektów JSONObject reprezentujaca otrzymane dane
	 * @throws Exception
	 *             w przypadku niepowodzenia przy konwertowaniu.
	 */
	public List<JSONObject> toJSONObjectsList(ResultSet rs) throws Exception {

		List<JSONObject> jsonObjects = new ArrayList<JSONObject>();

		try {
			ResultSetMetaData rsmd = rs.getMetaData();

			while (rs.next()) {
				int numColumns = rsmd.getColumnCount();
				JSONObject obj = new JSONObject();

				for (int i = 1; i < numColumns + 1; i++) {

					String column_name = rsmd.getColumnName(i);

					if (rsmd.getColumnType(i) == java.sql.Types.ARRAY) {
						obj.put(column_name, rs.getArray(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.BIGINT) {
						obj.put(column_name, rs.getInt(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.BOOLEAN) {
						obj.put(column_name, rs.getBoolean(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.BLOB) {
						obj.put(column_name, rs.getBlob(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.DOUBLE) {
						obj.put(column_name, rs.getDouble(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.FLOAT) {
						obj.put(column_name, rs.getFloat(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.INTEGER) {
						obj.put(column_name, rs.getInt(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.NVARCHAR) {
						obj.put(column_name, rs.getNString(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.VARCHAR) {
						obj.put(column_name, rs.getString(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.TINYINT) {
						obj.put(column_name, rs.getInt(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.SMALLINT) {
						obj.put(column_name, rs.getInt(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.DATE) {
						obj.put(column_name, rs.getDate(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.TIMESTAMP) {
						obj.put(column_name, rs.getTimestamp(column_name));
					} else if (rsmd.getColumnType(i) == java.sql.Types.NUMERIC) {
						obj.put(column_name, rs.getBigDecimal(column_name));
					} else {
						obj.put(column_name, rs.getObject(column_name));
					}
				}
				jsonObjects.add(obj);
			}
		} catch (Exception e) {
			e.printStackTrace();
		} finally {
			if (rs != null) {
				rs.close();
			}
		}

		return jsonObjects;
	}

}
