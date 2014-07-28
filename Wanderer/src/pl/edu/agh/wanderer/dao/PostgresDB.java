package pl.edu.agh.wanderer.dao;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;

import org.codehaus.jettison.json.JSONObject;

import pl.edu.agh.wanderer.util.JSONConverter;

public class PostgresDB extends DBConnection {
	private final JSONConverter toJsonConverter = new JSONConverter();

	public String getPlaceDesc(int placeId) {
		PreparedStatement query;
		Connection conn;
		String result = "";

		try {
			conn = getConnection();
			query = conn
					.prepareStatement("select \"primary description\" from metadata where metadata_id=?");
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
		/*
		 * for(JSONObject json : jsonObjects){
		 * getPointsAndUpdateSpecifiedJSONMetadata(json); }
		 */

		return toJsonConverter.convertListOfJSONObjects(jsonObjects);

	}

	public byte[] getPhoto(int photoId) {
		Connection connection = getConnection();
		byte[] result = null;
		try {
			PreparedStatement ps = connection
					.prepareStatement("SELECT photos.photo FROM photos inner join metadata on metadata.metadata_id=photos.metadata_id where metadata.metadata_id=?");
			ps.setInt(1, photoId);
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

	public byte[] getThumbnail(int photoId) {
		Connection connection = getConnection();
		byte[] result = null;
		try {
			PreparedStatement ps = connection
					.prepareStatement("SELECT photos.thumbnail FROM photos inner join metadata on metadata.metadata_id=photos.metadata_id where metadata.metadata_id=?");
			ps.setInt(1, photoId);
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

	public String getPointsWithinRange(String lon, String lat, String range) {
		Connection connection = getConnection();
		System.out.println("got an query");
		List<JSONObject> jsonObjects = new ArrayList<JSONObject>();
		try {
			PreparedStatement querry = connection			
					.prepareStatement("select metadata.coverage, metadata.orientation, photos.width, photos.height, metadata.metadata_id, metadata.longitude, metadata.latitude, metadata.primary_description, metadata.secondary_description, metadata.picture_hash, st_distance(metadata.geog,st_geogfromtext(?)) as distance from photos inner join metadata on (photos.metadata_id = metadata.metadata_id) where (st_dwithin(metadata.geog,st_geogfromtext(?),?)) order by 7;" );

			
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
			getPointsAndUpdateSpecifiedJSONMetadata(json);
		}

		return toJsonConverter.convertListOfJSONObjects(jsonObjects);
	}

	public String getPointsForSpecifiedMetadata(int metadata_id) {
		Connection connection = getConnection();
		List<JSONObject> jsonObjects = new ArrayList<JSONObject>();
		try {
			ResultSet result = null;

			int metadataID = metadata_id;
			// int metadataID = json.getInt("metadata_id");
			// System.out.println("metadataID: " + metadataID);

			PreparedStatement querry = connection
					.prepareStatement("select primary_description, secondary_description, category, x, y, alignment, line_length, angle , color from points where metadata_id = ? ;");
			querry.setInt(1, metadataID);
			result = querry.executeQuery();
			jsonObjects = toJsonConverter.toJSONObjectsList(result);

			// json.put("points", pointsInJSONArray);

		} catch (Exception e) {
			e.printStackTrace();
		} finally {
			try {
				connection.close();
			} catch (SQLException e) {
				e.printStackTrace();
			}
		}

		return toJsonConverter.convertListOfJSONObjects(jsonObjects);
	}

	public void getPointsAndUpdateSpecifiedJSONMetadata(JSONObject json) {
		Connection connection = getConnection();

		try {
			ResultSet result = null;

			int metadataID = json.getInt("metadata_id");
			System.out.println("metadataID: " + metadataID);

			PreparedStatement querry = connection
					.prepareStatement("select primary_description, secondary_description, category, x, y, alignment, line_length, angle from points where metadata_id = ? ;");
			querry.setInt(1, metadataID);
			result = querry.executeQuery();
			String pointsInJSONArray = toJsonConverter.toJSONArray(result)
					.toString();

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

}
