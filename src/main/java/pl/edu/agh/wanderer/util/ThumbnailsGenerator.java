package pl.edu.agh.wanderer.util;

import java.awt.image.BufferedImage;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;

import javax.imageio.ImageIO;

import org.imgscalr.Scalr;
import org.imgscalr.Scalr.Mode;

/**
 * Klasa utilowa sluzaca generowaniu miniaturek
 *
 */
public class ThumbnailsGenerator {

	private static final int THUMBNAIL_WIDTH = 600;
	private static final int THUMBNAIL_HEIGHT = 140;

	/**
	 * Metoda generujaca miniaturke zdjecia
	 * 
	 * @param image zdjecie
	 * @param width szerokosc zdjecia
	 * @param height wysokosc zdjecia
	 * @return miniaturka
	 * @throws IOException
	 */
	public static ByteArrayInputStream generateThumbnail(ByteArrayInputStream image, int width, int height) throws IOException {
		BufferedImage bufferedImage = ImageIO.read(image);
		image.reset();
		BufferedImage bufferedThumbnail = generateThumbnail(bufferedImage);
		ByteArrayOutputStream baos = new ByteArrayOutputStream();
		ImageIO.write(bufferedThumbnail, "jpg", baos);
		baos.flush();
		return new ByteArrayInputStream(baos.toByteArray());
	}

	/**
	 * Metoda wykonujaca proces stworzenia odpowiedniego bufora z miniaturka
	 * 
	 * @param img zdjecie jako bufor
	 * @return miniaturka jako bufor
	 */
	private static BufferedImage generateThumbnail(BufferedImage img) {
		double perfectScale = ((double) THUMBNAIL_WIDTH) / ((double) THUMBNAIL_HEIGHT);
		double actualScale = ((double) img.getWidth()) / ((double) img.getHeight());
		BufferedImage thumbnail = null;
		if (actualScale > perfectScale) {

			double scale = ((double) THUMBNAIL_HEIGHT) / ((double) img.getHeight());
			int newWidth = (int) (img.getWidth() * scale);
			int newHeight = (int) (img.getHeight() * scale);
			thumbnail = Scalr.resize(img, Mode.AUTOMATIC, newWidth, newHeight);
			if (thumbnail.getWidth() >= THUMBNAIL_WIDTH)
				thumbnail = thumbnail.getSubimage((thumbnail.getWidth() - THUMBNAIL_WIDTH) / 2, 0, THUMBNAIL_WIDTH,
						THUMBNAIL_HEIGHT);
		} else {
			double scale = ((double) THUMBNAIL_WIDTH) / ((double) img.getWidth());
			int newWidth = (int) (img.getWidth() * scale);
			int newHeight = (int) (img.getHeight() * scale);
			thumbnail = Scalr.resize(img, Mode.AUTOMATIC, newWidth, newHeight);
			if (thumbnail.getHeight() >= THUMBNAIL_HEIGHT)
				thumbnail = thumbnail.getSubimage(0, 0, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);

		}

		return thumbnail;
	}
}
