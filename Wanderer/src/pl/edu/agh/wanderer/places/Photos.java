package pl.edu.agh.wanderer.places;

import java.io.FileOutputStream;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import pl.edu.agh.wanderer.dao.PostgresDB;

@Path("/photos")
public class Photos {

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

		System.out.println(" Sending photo with id: " + photoId);

		PostgresDB dao = new PostgresDB();
		byte[] result = dao.getPhoto(Integer.parseInt(photoId));
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

		System.out.println("Sending thumbnail of photo with id  " + photoId);

		PostgresDB dao = new PostgresDB();
		byte[] result = dao.getThumbnail(Integer.parseInt(photoId));
		System.out.println(" Number of bytes: " + result.length);
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

		System.out.println("Received photo id " + photoId);
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

		System.out.println(" Sending metadata for place with id: " + placeId);
		PostgresDB dao = new PostgresDB();
		String myString = dao.getPhotoMetadata(Integer.parseInt(placeId));

		return myString;
	}
}
