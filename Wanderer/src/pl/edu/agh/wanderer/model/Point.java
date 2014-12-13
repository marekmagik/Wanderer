package pl.edu.agh.wanderer.model;

import org.codehaus.jettison.json.JSONException;
import org.codehaus.jettison.json.JSONObject;

public class Point {
	private String primaryDescription;
	private String secondaryDescription;
	private String category;
	private double x;
	private double y;
	private int alignment;
	private String color;
	private double lineLength;
	private double angle;
	
	public Point(String primaryDescription, String secondaryDescription, String category, double x, double y, int alignment,
			String color, double lineLength, double angle) {
		this.primaryDescription = primaryDescription;
		this.secondaryDescription = secondaryDescription;
		this.category = category;
		this.x = x;
		this.y = y;
		this.alignment = alignment;
		this.color = color;
		this.lineLength = lineLength;
		this.angle = angle;
	}
	
	public Point(JSONObject json) throws JSONException{
		this.primaryDescription = json.getString("PrimaryDescription");
		this.secondaryDescription = json.getString("SecondaryDescription");
		JSONObject categoryObject = json.getJSONObject("Category");
		this.category = categoryObject.getString("Name");
		this.x = json.getDouble("X");
		this.y = json.getDouble("Y");
		this.alignment = json.getInt("Alignment");
		this.color = json.getString("Color");
		this.lineLength = json.getDouble("LineLength");
		this.angle = json.getDouble("Angle");
	}

	public String getPrimaryDescription() {
		return primaryDescription;
	}

	public String getSecondaryDescription() {
		return secondaryDescription;
	}

	public String getCategory() {
		return category;
	}

	public double getX() {
		return x;
	}

	public double getY() {
		return y;
	}

	public int getAlignment() {
		return alignment;
	}

	public String getColor() {
		return color;
	}

	public double getLineLength() {
		return lineLength;
	}

	public double getAngle() {
		return angle;
	}

	@Override
	public String toString() {
		return "Point [primaryDescription=" + primaryDescription + ", secondaryDescription=" + secondaryDescription
				+ ", category=" + category + ", x=" + x + ", y=" + y + ", alignment=" + alignment + ", color=" + color
				+ ", lineLength=" + lineLength + ", angle=" + angle + "]";
	}
	
	
}
