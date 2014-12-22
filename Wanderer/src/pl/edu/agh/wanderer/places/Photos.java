package pl.edu.agh.wanderer.places;

import java.io.FileOutputStream;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.security.NoSuchAlgorithmException;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.codehaus.jettison.json.JSONException;

import pl.edu.agh.wanderer.dao.PostgresDB;

@Path("/photos")
public class Photos {

	private final Logger logger = LogManager.getLogger(Photos.class); 
	
	/**
	 * Metoda zwracajaca dane zdjêcie w postaci tablicy bajtów.
	 * 
	 * @param photoId
	 *            id zdjêcia, które chcemy pobraæ
	 * @return obiekt Response zawierajcy zdjêcie
	 * @throws Exception
	 *             w przypadku niepowodzenia w trakcie pobierania obiektu z bazy
	 */
	@Path("/get/{id}")
	@GET
	@Produces({ "image/jpeg" })
	public Response getPlaceDesc(@PathParam("id") String photoId) throws Exception {

		logger.debug(" Sending photo with id: " + photoId);

		PostgresDB dao = new PostgresDB();
		byte[] result = dao.getPhoto(photoId);
		return Response.ok(result).build();

	}
	
	@Path("/get/waiting/{id}")
	@GET
	@Produces({ "image/jpeg" })
	public Response getImageFromWaitingRoom(@PathParam("id") String photoId) throws Exception {

		logger.debug(" Sending photo with id: " + photoId);

		PostgresDB dao = new PostgresDB();
		byte[] result = dao.getPhotoFromWaitingRoom(photoId);
		return Response.ok(result).build();

	}

	/**
	 * Metoda zwracajaca miniaturkê danego zdjêcia w postaci tablicy bajtów.
	 * 
	 * @param photoId
	 *            id zdjêcia, którego miniaturkê chcemy pobraæ
	 * @return obiekt Response zawierajcy miniaturkê
	 * @throws Exception
	 *             w przypadku blêdnego odczytu z bazy
	 */
	@Path("/get/thumbnail/{id}")
	@GET
	@Produces({ "image/jpeg" })
	public Response getPlaceThumbnail(@PathParam("id") String photoId) throws Exception {

		logger.debug("Sending thumbnail of photo with id  " + photoId);

		PostgresDB dao = new PostgresDB();
		byte[] result = dao.getThumbnail(photoId);
		logger.debug(" Number of bytes: " + result.length);
		return Response.ok(result).build();

	}
	
	@Path("/get/waiting/thumbnail/{id}")
	@GET
	@Produces({ "image/jpeg" })
	public Response getPlaceThumbnailFromWaitingRoom(@PathParam("id") String photoId) throws Exception {

		logger.debug("Sending thumbnail of photo with id  " + photoId);

		PostgresDB dao = new PostgresDB();
		byte[] result = dao.getThumbnailFromWaitingRoom(photoId);
		logger.debug(" Number of bytes: " + result.length);
		return Response.ok(result).build();

	}

	/**
	 * Testowa metoda do odbierania wyslanego zdjecia. W przyszloœci bêdzie
	 * sluzyla do dodawania zdjecia do bazy danych.
	 * 
	 * @param photoId
	 *            id otrzymanego zdjêcia - parametr zostanie wyrzucony
	 * @param incomingData
	 *            zdjecie w postaci tablicy bajtow
	 * @return odpowiedni kod, 200 - sukces, 500 - porazka
	 * @throws Exception
	 *             w przypadku niepowodzenia przy zapisie pliku
	 */
	@Path("/set/photo/{id}")
	@POST
	@Consumes("image/jpeg")
	@Produces(MediaType.TEXT_PLAIN)
	public String setPlacePhoto(@PathParam("id") String photoId, byte[] incomingData) throws Exception {

		logger.debug("Received photo id " + photoId);
		FileOutputStream fos = new FileOutputStream("D:/img/recv.jpg");
		fos.write(incomingData);
		fos.close();

		return "200";

	}

	/**
	 * Metoda zwraca metadane dla opisujace miejsce o danym id.
	 * 
	 * @param placeId
	 *            id miejsca, ktorego metadane chcemy otrzymac
	 * @return metadane w formacie JSON
	 * @throws Exception
	 *             w przypadku niepowodzenia odczytu z bazy danych
	 */
	@Path("/get/meta/{id}")
	@GET
	@Produces(MediaType.TEXT_PLAIN)
	public String getPhotoMetadata(@PathParam("id") String placeId) throws Exception {

		logger.debug(" Sending metadata for place with id: " + placeId);
		PostgresDB dao = new PostgresDB();
		String myString = dao.getPhotoMetadata(Integer.parseInt(placeId));

		return myString;
	}

	/**
	 * Metoda wstawiajaca do bazy komplet danych o miejscu
	 * (lacznie ze zdjeciem i miniaturka)
	 * 
	 * @param mode tryp wstawiania
	 * @param metadataLength ilosc bajtow metadanych
	 * @param imageLength ilosc bajtow zdjecia
	 * @param input closc przeslanych danych
	 * @return status wykonania, 200 - OK, 500 - ERROR
	 */
	@Path("/insert/{mode}/{metadataLength}/{imageLength}")
	@POST
	@Consumes(MediaType.APPLICATION_OCTET_STREAM)
	public String uploadPhotoWithMetadata(@PathParam("mode") String mode, @PathParam("metadataLength") String metadataLength,
			@PathParam("imageLength") String imageLength, byte[] input) {
		int lengthOfMetadata = 0;
		int lengthOfImage = 0;
		try {
			lengthOfMetadata = Integer.parseInt(metadataLength);
			lengthOfImage = Integer.parseInt(imageLength);
		} catch (NumberFormatException ex) {
			return "500";
		}

		byte[] json = new byte[lengthOfMetadata];
		byte[] image = new byte[lengthOfImage];

		try {
			for (int i = 0; i < lengthOfMetadata; i++)
				json[i] = input[i];

			for (int i = 0; i < lengthOfImage; i++)
				image[i] = input[i + lengthOfMetadata];
		} catch (IndexOutOfBoundsException e) {
			return "500";
		}

		String metadata;
		try {
			metadata = new String(json, "UTF-8");
		} catch (UnsupportedEncodingException e) {
			return "500";
		}
		logger.debug(metadata);

		PostgresDB dao = new PostgresDB();
		try {
			dao.insertPhotoAndMetadata(image, metadata, mode);
		} catch (JSONException | IOException | NoSuchAlgorithmException e) {
			e.printStackTrace();
			return "500";
		}

		logger.debug(" New photo and metadata inserted ");
		return "200";
	}
}
