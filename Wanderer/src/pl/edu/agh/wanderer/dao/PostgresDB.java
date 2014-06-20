package pl.edu.agh.wanderer.dao;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;

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
		String result = "";

		try {
			conn = getConnection();
			query = conn
					.prepareStatement("select md.coverage, ph.width, ph.height from photos as ph inner join metadata as md on md.metadata_id=ph.metadata_id where ph.metadata_id=?");
			query.setInt(1, placeId);
			ResultSet resultSet = query.executeQuery();
			ToJSON toJSON = new ToJSON();
			JSONArray json = toJSON.toJSONArray(resultSet);
			result=json.toString();
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
	
	public String getPointsWithinRange(String lon, String lat, String range){
		Connection connection = getConnection();
		String result = null;
		System.out.println("got an query");
		try {
			PreparedStatement querry = connection.prepareStatement("select metadata_id,longitude,latitude,\"primary_description\", \"secondary_description\", picture_hash , st_distance(metadata.geog,st_geogfromtext(?)) as distance from metadata where st_dwithin(metadata.geog,st_geogfromtext(?),?) order by 7;");
			querry.setString(1, "srid=4326;point("+lon+" "+lat+")");
			querry.setString(2, "srid=4326;point("+lon+" "+lat+")");
			querry.setInt(3, Integer.parseInt(range));
			ResultSet rs = querry.executeQuery();
			ToJSON toJSON = new ToJSON();
			JSONArray json = toJSON.toJSONArray(rs);
			result=json.toString();
			connection.close();
					
		} catch (Exception e) {
			e.printStackTrace();
		}
		
		
		return result;
	}
}
