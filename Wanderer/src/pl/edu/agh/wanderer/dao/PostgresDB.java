package pl.edu.agh.wanderer.dao;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;

import org.codehaus.jettison.json.JSONArray;

import pl.edu.agh.wanderer.util.ToJSON;

public class PostgresDB extends DBConnection {

	public String getPlaceDesc(int placeId) {
		PreparedStatement query;
		Connection conn;
		String result = "";

		try {
			conn = getConnection();
			query = conn
					.prepareStatement("select \"desc\" from places where place_id=?");
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
		String result = "";

		try {
			conn = getConnection();
			query = conn
					.prepareStatement("select ph.perc, ph.width, ph.height from photos as ph where ph.place_id=?");
			query.setInt(1, placeId);
			ResultSet resultSet = query.executeQuery();
			ToJSON toJSON = new ToJSON();
			JSONArray json = toJSON.toJSONArray(resultSet);
			result = json.toString();
			conn.close();

		} catch (Exception ex) {
			ex.printStackTrace();
		}

		return result;
	}

	public byte[] getPhoto(int photoId) {
		Connection connection = getConnection();
		byte[] result = null;
		try {
			PreparedStatement ps = connection
					.prepareStatement("SELECT photos.photo FROM photos inner join places on places.place_id=photos.place_id where places.place_id=?");
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
					.prepareStatement("SELECT photos.thumbnail FROM photos inner join places on places.place_id=photos.place_id where places.place_id=?");
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

	public String getPointsWithinRannge(String lon, String lat, String range) {
		Connection connection = getConnection();
		String result = null;

		try {
			PreparedStatement querry = connection
					.prepareStatement("select p.place_id,p.lon,p.lat,p.\"desc\",ph.hash,st_distance(p.geog,st_geogfromtext(?)) as distance from places as p inner join photos as ph on p.place_id=ph.place_id where st_dwithin(p.geog,st_geogfromtext(?),?) order by 5;");
			querry.setString(1, "srid=4326;point(" + lon + " " + lat + ")");
			querry.setString(2, "srid=4326;point(" + lon + " " + lat + ")");
			querry.setInt(3, Integer.parseInt(range));
			ResultSet rs = querry.executeQuery();
			ToJSON toJSON = new ToJSON();
			JSONArray json = toJSON.toJSONArray(rs);
			result = json.toString();
			connection.close();

		} catch (Exception e) {
			e.printStackTrace();
		}

		return result;
	}

	public String getPointsWithinRanngeWithHash(String lon, String lat,
			String range, String hash) {
		Connection connection = getConnection();
		String result = null;

		try {
			String listOfHash=hash.replace(",","','");
			listOfHash="'"+listOfHash+"'";
			PreparedStatement querry = connection
					.prepareStatement("select p.place_id,p.lon,p.lat,p.\"desc\",ph.hash,st_distance(p.geog,st_geogfromtext(?)) as distance from places as p inner join photos as ph on p.place_id=ph.place_id where st_dwithin(p.geog,st_geogfromtext(?),?) and ph.hash not in ("+listOfHash+") order by 5;");
			querry.setString(1, "srid=4326;point(" + lon + " " + lat + ")");
			querry.setString(2, "srid=4326;point(" + lon + " " + lat + ")");
			querry.setInt(3, Integer.parseInt(range));
			
			ResultSet rs = querry.executeQuery();
			ToJSON toJSON = new ToJSON();
			JSONArray json = toJSON.toJSONArray(rs);
			result = json.toString();
			connection.close();

		} catch (Exception e) {
			e.printStackTrace();
		}

		return result;
	}
}
