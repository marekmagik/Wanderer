package pl.edu.agh.wanderer.dao;

import java.io.IOException;
import java.security.NoSuchAlgorithmException;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;

import org.codehaus.jettison.json.JSONArray;
import org.codehaus.jettison.json.JSONException;
import org.codehaus.jettison.json.JSONObject;

import pl.edu.agh.wanderer.model.Metadata;
import pl.edu.agh.wanderer.model.Photo;
import pl.edu.agh.wanderer.model.Point;
import pl.edu.agh.wanderer.util.JSONConverter;

/**
 * Klasa odpowiadajaca za wszystkie operacje zwiazane z baza danych
 */
public class PostgresDB extends DBConnection {
	private final JSONConverter toJsonConverter = new JSONConverter();

	/**
	 * Metoda dokonujaca bezposredniego zapytania na bazie danych w celu
	 * uzyskania opisu miejsca. Metoda w celach testowych.
	 * 
	 * @param placeId
	 *            id miejsca, ktorego opis chcemy uzyskac
	 * @return opis miejsca
	 */
	public String getPlaceDesc(int placeId) {
		PreparedStatement query;
		Connection conn;
		String result = "";

		try {
			conn = getConnection();
			query = conn.prepareStatement("select \"primary description\" from metadata where metadata_id=?");
			query.setInt(1, placeId);
			ResultSet resultSet = query.executeQuery();
			while (resultSet.next()) {
				result = resultSet.getString(1);
			}
			conn.close();
		} catch (Exception ex) {
			ex.printStackTrace();
		}

		return result;
	}

	/**
	 * Metoda wykonujaca bezposrednie zapytanie do bazy danych w celu uzyskania
	 * metadanych dla danego miejsca
	 * 
	 * @param placeId
	 *            id miejsca, ktorego metadane chcemy uzyskac
	 * @return metadane danego miejsca w formacie JSON
	 */
	public String getPhotoMetadata(int placeId) {
		PreparedStatement query;
		Connection conn;
		List<JSONObject> jsonObjects = new ArrayList<JSONObject>();

		try {
			conn = getConnection();
			query = conn
					.prepareStatement("select md.coverage, md.orientation, ph.width, ph.height from photos as ph inner join metadata as md on md.metadata_id=ph.metadata_id where ph.metadata_id=?");
			query.setInt(1, placeId);
			ResultSet rs = query.executeQuery();

			jsonObjects = toJsonConverter.toJSONObjectsList(rs);
			conn.close();
		} catch (Exception e) {
			e.printStackTrace();
		}

		return toJsonConverter.convertListOfJSONObjects(jsonObjects);

	}

	/**
	 * Metoda wykonujaca zapytanie do bazy danych w celu pobrania zdjecia o
	 * odpowiednim id
	 * 
	 * @param photoId
	 *            id danego zdjecia
	 * @return zdjecie jako tablica bajtow
	 */
	public byte[] getPhoto(String photoId) {
		Connection connection = getConnection();
		byte[] result = null;
		try {
			PreparedStatement ps = connection
					.prepareStatement("SELECT photos.photo FROM photos inner join metadata on metadata.metadata_id=photos.metadata_id where metadata.picture_hash=?");
			ps.setString(1, photoId);
			ResultSet rs = ps.executeQuery();
			if (rs != null) {
				while (rs.next()) {
					result = rs.getBytes(1);

				}
				rs.close();
			}
			ps.close();
			connection.close();
		} catch (Exception ex) {
			ex.printStackTrace();
		}

		return result;
	}
	
	public byte[] getPhotoFromWaitingRoom(String photoId) {
		Connection connection = getConnection();
		byte[] result = null;
		try {
			PreparedStatement ps = connection
					.prepareStatement("SELECT photos.photo FROM photos_waiting_room as photos inner join metadata_waiting_room as metadata on metadata.metadata_id=photos.metadata_id where metadata.picture_hash=?");
			ps.setString(1, photoId);
			ResultSet rs = ps.executeQuery();
			if (rs != null) {
				while (rs.next()) {
					result = rs.getBytes(1);

				}
				rs.close();
			}
			ps.close();
			connection.close();
		} catch (Exception ex) {
			ex.printStackTrace();
		}

		return result;
	}

	/**
	 * Metoda wykonujaca zapytanie na bazie danych w celu uzyskania miniatruki
	 * zjdecia o podanym id
	 * 
	 * @param photoId
	 *            id zdjecia, ktorego miniatruke chcemy uzyskac
	 * @return miniatruka jako tablica bajtow
	 */
	public byte[] getThumbnail(String photoId) {
		Connection connection = getConnection();
		byte[] result = null;
		try {
			PreparedStatement ps = connection
					.prepareStatement("SELECT photos.thumbnail FROM photos inner join metadata on metadata.metadata_id=photos.metadata_id where metadata.picture_hash=?");
			ps.setString(1, photoId);
			ResultSet rs = ps.executeQuery();
			if (rs != null) {
				while (rs.next()) {
					result = rs.getBytes(1);

				}
				rs.close();
			}
			ps.close();
			connection.close();
		} catch (Exception ex) {
			ex.printStackTrace();
		}

		return result;
	}
	
	public byte[] getThumbnailFromWaitingRoom(String photoId) {
		Connection connection = getConnection();
		byte[] result = null;
		try {
			PreparedStatement ps = connection
					.prepareStatement("SELECT photos.thumbnail FROM photos_waiting_room as photos inner join metadata_waiting_room as metadata on metadata.metadata_id=photos.metadata_id where metadata.picture_hash=?");
			ps.setString(1, photoId);
			ResultSet rs = ps.executeQuery();
			if (rs != null) {
				while (rs.next()) {
					result = rs.getBytes(1);

				}
				rs.close();
			}
			ps.close();
			connection.close();
		} catch (Exception ex) {
			ex.printStackTrace();
		}

		return result;
	}

	/**
	 * Metoda zwracajaca miejsca znajdujace sie w zadanym promieniu od podanego
	 * punktu.
	 * 
	 * @param lon
	 *            dlugosc geograficzna punktu
	 * @param lat
	 *            szerokosc geograficzna punktu
	 * @param range
	 *            promien w metrach
	 * @return lista miejsc wraz z metadanymi w formacie JSON
	 */
	public String getPointsWithinRange(String lon, String lat, String range) {
		Connection connection = getConnection();
		System.out.println("got an query");
		List<JSONObject> jsonObjects = new ArrayList<JSONObject>();
		try {
			PreparedStatement querry = connection
					.prepareStatement("select metadata.coverage, metadata.orientation, photos.width, photos.height, metadata.metadata_id, metadata.longitude, metadata.latitude, metadata.primary_description, metadata.secondary_description, metadata.picture_hash, metadata.category, st_distance(metadata.geog,st_geogfromtext(?)) as distance from photos inner join metadata on (photos.metadata_id = metadata.metadata_id) where (st_dwithin(metadata.geog,st_geogfromtext(?),?)) order by 7;");

			querry.setString(1, "srid=4326;point(" + lon + " " + lat + ")");
			querry.setString(2, "srid=4326;point(" + lon + " " + lat + ")");
			querry.setInt(3, Integer.parseInt(range));
			ResultSet rs = querry.executeQuery();
			jsonObjects = toJsonConverter.toJSONObjectsList(rs);
			connection.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
		for (JSONObject json : jsonObjects) {
			getPointsAndUpdateSpecifiedJSONMetadata(json,"normal");
		}

		return toJsonConverter.convertListOfJSONObjects(jsonObjects);
	}

	/**
	 * Metoda pobierajaca punkty (opisujace zdjecie dla danego miejsca) i
	 * aktualizujaca obiekt JSON z uzyciem uzyskanych danych.
	 * 
	 * @param json
	 *            obiekt JSON, zawierajacy liste miejsc wraz z metadanymi
	 */
	public void getPointsAndUpdateSpecifiedJSONMetadata(JSONObject json, String mode) {
		Connection connection = getConnection();

		String query="";
		if("normal".equals(mode))
			query="select primary_description, secondary_description, category, x, y, alignment, line_length, angle, color from points where metadata_id = ? ;";
		else if("admin".equals(mode))
			query="select primary_description, secondary_description, category, x, y, alignment, line_length, angle, color from points_waiting_room where metadata_id = ? ;";
		
		try {
			ResultSet result = null;

			int metadataID = json.getInt("metadata_id");
			System.out.println("metadataID: " + metadataID);

			PreparedStatement querry = connection
					.prepareStatement(query);
			querry.setInt(1, metadataID);
			result = querry.executeQuery();
			String pointsInJSONArray = toJsonConverter.toJSONArray(result).toString();

			json.put("points", pointsInJSONArray);

		} catch (Exception e) {
			e.printStackTrace();
		} finally {
			try {
				connection.close();
			} catch (SQLException e) {
				e.printStackTrace();
			}
		}
	}

	public boolean insertPhotoAndMetadata(byte[] image, String metadata, String mode) throws JSONException, IOException,
			NoSuchAlgorithmException {
		JSONConverter converter = new JSONConverter();
		Metadata metadataObject = converter.toMetadataObject(metadata, image);
		Connection connection = getConnection();

		String metadataQuery = "";
		String photoQuery = "";
		String pointsQuery = "";
		
		if("admin".equals(mode)){
			metadataQuery="insert into metadata values (default,?, ? ,? , ?, ?, ?, ?, ?, st_geogfromtext(?), ?);";
			photoQuery="INSERT INTO photos VALUES (default , ?, ? , ?, ? ,?);";
			pointsQuery="insert into points values (default,?, ? ,? , ?, ?, ?, ?, ?, ?, ?);";
		}else if("normal".equals(mode)){
			metadataQuery="insert into metadata_waiting_room values (default,?, ? ,? , ?, ?, ?, ?, ?, st_geogfromtext(?), ?);";
			photoQuery="INSERT INTO photos_waiting_room VALUES (default , ?, ? , ?, ? ,?);";
			pointsQuery="insert into points_waiting_room values (default,?, ? ,? , ?, ?, ?, ?, ?, ?, ?);";
		} else
			return false;
		
		try {
			PreparedStatement querry = connection
					.prepareStatement(metadataQuery);
			querry.setString(1, metadataObject.getPrimaryDescription());
			querry.setString(2, metadataObject.getSecondaryDescription());
			querry.setDouble(3, metadataObject.getLongitude());
			querry.setDouble(4, metadataObject.getLatitude());
			querry.setDouble(5, metadataObject.getCoverage());
			querry.setDouble(6, metadataObject.getOrientation());
			querry.setDouble(7, metadataObject.getVersion());
			querry.setString(8, metadataObject.getHash());
			querry.setString(9, "srid=4326;point(" + metadataObject.getLongitude() + " " + metadataObject.getLatitude() + ")");
			querry.setString(10, metadataObject.getCategory());
			querry.executeUpdate();
		} catch (SQLException e) {
			e.printStackTrace();
			return false;
		} finally {
			try {
				connection.close();
			} catch (SQLException e) {
				e.printStackTrace();
				return false;
			}
		}

		int metadataIndex = getMetadataIndex(metadataObject.getHash(),mode);
		if (metadataIndex == 0)
			return false;

		Photo photo = metadataObject.getPhoto();
		connection = getConnection();
		try {
			PreparedStatement querry = connection.prepareStatement(photoQuery);
			querry.setBinaryStream(1, photo.getPhoto(), photo.getPhoto().available());
			querry.setBinaryStream(2, photo.getThumbnail(), photo.getThumbnail().available());
			querry.setInt(3, photo.getWidth());
			querry.setInt(4, photo.getHeight());
			querry.setInt(5, metadataIndex);
			querry.executeUpdate();
		} catch (SQLException e) {
			e.printStackTrace();
			return false;
		} finally {
			try {
				connection.close();
			} catch (SQLException e) {
				e.printStackTrace();
				return false;
			}
		}

		for (Point point : metadataObject.getPoints()) {
			connection = getConnection();
			try {
				PreparedStatement querry = connection
						.prepareStatement(pointsQuery);
				querry.setString(1, point.getPrimaryDescription());
				querry.setString(2, point.getSecondaryDescription());
				querry.setString(3, point.getCategory());
				querry.setInt(4, metadataIndex);
				querry.setInt(5, (int) point.getX());
				querry.setInt(6, (int) point.getY());
				querry.setInt(7, point.getAlignment());
				querry.setInt(8, (int) point.getLineLength());
				querry.setInt(9, (int) point.getAngle());
				querry.setString(10, point.getColor());
				querry.executeUpdate();
			} catch (SQLException e) {
				e.printStackTrace();
				return false;
			} finally {
				try {
					connection.close();
				} catch (SQLException e) {
					e.printStackTrace();
					return false;
				}
			}
		}

		return true;
	}

	private int getMetadataIndex(String hash, String mode) {
		Connection connection = getConnection();
		
		String query="";
		if("admin".equals(mode))
			query="select metadata_id from metadata where picture_hash=?";
		else if("normal".equals(mode))
			query="select metadata_id from metadata_waiting_room where picture_hash=?";
		
		int result = 0;
		try {
			PreparedStatement querry = connection.prepareStatement(query);
			querry.setString(1, hash);
			ResultSet rs = querry.executeQuery();
			if (rs.next())
				result = rs.getInt(1);
		} catch (SQLException e) {
			e.printStackTrace();
			return result;
		} finally {
			try {
				connection.close();
			} catch (SQLException e) {
				e.printStackTrace();
				return result;
			}
		}
		return result;
	}

	public String getAllPointsFromWaitingRoom() {
		Connection connection = getConnection();
		System.out.println("got an query");
		List<JSONObject> jsonObjects = new ArrayList<JSONObject>();
		try {
			PreparedStatement querry = connection
					.prepareStatement("select metadata.coverage, metadata.orientation, photos.width, photos.height, metadata.metadata_id, metadata.longitude, metadata.latitude, metadata.primary_description, metadata.secondary_description, metadata.picture_hash, metadata.category from photos_waiting_room as photos inner join metadata_waiting_room as metadata on (photos.metadata_id = metadata.metadata_id)");

			ResultSet rs = querry.executeQuery();
			jsonObjects = toJsonConverter.toJSONObjectsList(rs);
			connection.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
		for (JSONObject json : jsonObjects) {
			getPointsAndUpdateSpecifiedJSONMetadata(json,"admin");
		}

		return toJsonConverter.convertListOfJSONObjects(jsonObjects);
	}

	public boolean deletePlaceFromWaitingRoom(String hash) {
		Connection connection = getConnection();
		boolean result = true;
		int metadataIndex = getMetadataIndex(hash, "normal");
		try {
			PreparedStatement querry = connection
					.prepareStatement("delete from points_waiting_room where metadata_id=?");

			querry.setInt(1, metadataIndex);
			querry.execute();
			
			querry = connection
					.prepareStatement("delete from photos_waiting_room where metadata_id=?");

			querry.setInt(1, metadataIndex);
			querry.execute();
			
			querry = connection
					.prepareStatement("delete from metadata_waiting_room where metadata_id=?");

			querry.setInt(1, metadataIndex);
			querry.execute();
			
		} catch (Exception e) {
			e.printStackTrace();
			result=false;
		} finally{
			try {
				connection.close();
			} catch (SQLException e) {
				e.printStackTrace();
			}
		}

		return result;
	}
	
	public String getPlacesWithCategory(String category) {
		Connection connection = getConnection();
		List<JSONObject> jsonObjects = new ArrayList<JSONObject>();
		try {
			PreparedStatement querry = connection
					.prepareStatement("select metadata.coverage, metadata.orientation, photos.width, photos.height, metadata.metadata_id, metadata.longitude, metadata.latitude, metadata.primary_description, metadata.secondary_description, metadata.picture_hash, metadata.category from photos inner join metadata on (photos.metadata_id = metadata.metadata_id) where metadata.category=?");

			querry.setString(1,category);
			ResultSet rs = querry.executeQuery();
			jsonObjects = toJsonConverter.toJSONObjectsList(rs);
			connection.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
		for (JSONObject json : jsonObjects) {
			getPointsAndUpdateSpecifiedJSONMetadata(json,"normal");
		}

		return toJsonConverter.convertListOfJSONObjects(jsonObjects);
	}
	
	public String getAllPlacesCategories(){
		Connection connection = getConnection();
		String resultJson = "{}";
		try {
			PreparedStatement query = connection
					.prepareStatement("select distinct category from metadata");
			ResultSet result = query.executeQuery();
			JSONConverter converter = new JSONConverter();
			JSONArray jsonArray = converter.toJSONArray(result);
			resultJson = jsonArray.toString();
			
		} catch (Exception e) {
			e.printStackTrace();
		} finally{
			try {
				connection.close();
			} catch (SQLException e) {
				e.printStackTrace();
			}
		}
		
		return resultJson;
	}

}
