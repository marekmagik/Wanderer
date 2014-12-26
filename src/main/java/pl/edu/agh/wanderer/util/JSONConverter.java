package pl.edu.agh.wanderer.util;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.security.NoSuchAlgorithmException;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.util.ArrayList;
import java.util.List;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.codehaus.jettison.json.JSONArray;
import org.codehaus.jettison.json.JSONException;
import org.codehaus.jettison.json.JSONObject;

import pl.edu.agh.wanderer.model.Metadata;
import pl.edu.agh.wanderer.model.Photo;
import pl.edu.agh.wanderer.model.Point;

/**
 * Klasa odpowiadajaca za konwertowanie wyników zapytania do baza danych na dane
 * w formacie JSON.
 * 
 */
public class JSONConverter {

	private final Logger logger = LogManager.getLogger(JSONConverter.class);
	
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

	/**
	 * Metoda konwertujaca json w postaci napisu do obiektu JSONObject
	 * 
	 * @param json json
	 * @return json jako obiekt JSONObject
	 * @throws JSONException
	 */
	public JSONObject toJSONArray(String json) throws JSONException {
		return new JSONObject(json);
	}

	/**
	 * Metoda konwertujaca zestaw danych o miejscu (zapisany w json'ie i tablicy bajtow)
	 * do obiektu Metadanych 
	 * 
	 * @param json json opisujacy miejsce
	 * @param image zdjecie jako tablica bajtow
	 * @return zainicjalizowany obiekt metadanych
	 * @throws JSONException
	 * @throws IOException
	 * @throws NoSuchAlgorithmException
	 */
	public Metadata toMetadataObject(String json, byte [] image) throws JSONException, IOException, NoSuchAlgorithmException{
		JSONObject metadataJson = new JSONObject(json);
		JSONArray pointsJson = metadataJson.getJSONArray("Points");
		
		List<Point> points = new ArrayList<Point>();
		
		for(int i=0;i<pointsJson.length();i++){
			JSONObject pointJson = pointsJson.getJSONObject(i);
			Point point = new Point(pointJson);
			logger.debug(point);
			points.add(point);
		}
		
		int width = metadataJson.getInt("Width");
		int height = metadataJson.getInt("Height");
		ByteArrayInputStream imageInputStream = new ByteArrayInputStream(image);
		ByteArrayInputStream thumbnailInoutStream = ThumbnailsGenerator.generateThumbnail(imageInputStream, width, height);
		Photo photo = new Photo(imageInputStream,thumbnailInoutStream,width,height);
		logger.debug(photo);
		
		Metadata metadata = new Metadata(metadataJson,photo,points);
		logger.debug(metadata);
		
		return metadata;
	}
	
}
