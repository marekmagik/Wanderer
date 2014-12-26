package pl.edu.agh.wanderer.model;

import java.io.ByteArrayInputStream;

/**
 * Klasa reprezentujaca zdjecie
 */
public class Photo {

	private ByteArrayInputStream photo;
	private ByteArrayInputStream thumbnail;
	private int width;
	private int height;
	
	public Photo(ByteArrayInputStream photo, ByteArrayInputStream thumbnail, int width, int height) {
		this.photo = photo;
		this.thumbnail = thumbnail;
		this.width = width;
		this.height = height;
	}
	
	public ByteArrayInputStream getPhoto() {
		return photo;
	}

	public ByteArrayInputStream getThumbnail() {
		return thumbnail;
	}

	public int getWidth() {
		return width;
	}

	public int getHeight() {
		return height;
	}

	@Override
	public String toString() {
		return "Photo [photo=" + photo.available() + ", thumbnail=" + thumbnail.available() + ", width=" + width + ", height=" + height + "]";
	}
	
	
	
}
