package pl.edu.agh.wanderer.model;

import java.io.IOException;
import java.security.NoSuchAlgorithmException;
import java.util.List;

import org.codehaus.jettison.json.JSONException;
import org.codehaus.jettison.json.JSONObject;

import pl.edu.agh.wanderer.util.HashGenerator;

/**
 * Klasa reprezentujaca metadane 
 */
public class Metadata {
	private String primaryDescription;
	private String secondaryDescription;
	private double longitude;
	private double latitude;
	private double coverage;
	private double orientation;
	private double version;
	private String hash;
	private Photo photo;
	private List<Point> points;
	private String category;

	public Metadata(String primaryDescription, String secondaryDescription, double longitude, double latitude, double coverage,
			double orientation, double version, String hash, Photo photo, List<Point> points, String category) {
		this.primaryDescription = primaryDescription;
		this.secondaryDescription = secondaryDescription;
		this.longitude = longitude;
		this.latitude = latitude;
		this.coverage = coverage;
		this.orientation = orientation;
		this.version = version;
		this.hash = hash;
		this.photo = photo;
		this.points = points;
		this.category = category;
	}

	public Metadata(JSONObject metadataJson, Photo photo, List<Point> points) throws JSONException, NoSuchAlgorithmException, IOException {
		this.primaryDescription = metadataJson.getString("PictureDescription");
		this.secondaryDescription = metadataJson.getString("PictureAdditionalDescription");
		this.longitude = metadataJson.getDouble("Longitude");
		this.latitude = metadataJson.getDouble("Latitude");
		this.coverage = metadataJson.getDouble("CoverageInPercent");
		this.orientation = metadataJson.getDouble("OrientationOfLeftBorder");
		this.version = metadataJson.getDouble("Version");
		this.hash = HashGenerator.generateHash(photo.getPhoto());
		this.photo = photo;
		this.points = points;
		this.category = metadataJson.getString("Category");
	}

	public String getPrimaryDescription() {
		return primaryDescription;
	}

	public String getSecondaryDescription() {
		return secondaryDescription;
	}

	public double getLongitude() {
		return longitude;
	}

	public double getLatitude() {
		return latitude;
	}

	public double getCoverage() {
		return coverage;
	}

	public double getOrientation() {
		return orientation;
	}

	public double getVersion() {
		return version;
	}

	public String getHash() {
		return hash;
	}

	public Photo getPhoto() {
		return photo;
	}

	public List<Point> getPoints() {
		return points;
	}

	public String getCategory() {
		return category;
	}

	@Override
	public String toString() {
		return "Metadata [primaryDescription=" + primaryDescription + ", secondaryDescription=" + secondaryDescription
				+ ", longitude=" + longitude + ", latitude=" + latitude + ", coverage=" + coverage + ", orientation="
				+ orientation + ", version=" + version + ", hash=" + hash + ", photo=" + photo + ", points=" + points
				+ ", category=" + category + "]";
	}

	
	
}
