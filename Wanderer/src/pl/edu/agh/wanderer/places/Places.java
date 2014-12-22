package pl.edu.agh.wanderer.places;

import javax.ws.rs.DELETE;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import pl.edu.agh.wanderer.dao.PostgresDB;

@Path("/places")
public class Places {

	private final Logger logger = LogManager.getLogger(Places.class);
	
	/**
	 * Metoda zwracajace opis dla miejsca o danym id. Metoda do celow testowych.
	 * 
	 * @param placeId
	 *            id miejsca, ktorego opis chcemy otrzymac
	 * @return opis miejsca
	 * @throws Exception
	 *             w przypadku nieudanego odczytu z bazy danych
	 */
	@Path("/get/{id}")
	@GET
	@Produces(MediaType.TEXT_PLAIN)
	public String getPlaceDesc(@PathParam("id") String placeId) throws Exception {

		logger.debug(" Sending description for place with id " + placeId);
		PostgresDB dao = new PostgresDB();
		String myString = dao.getPlaceDesc(Integer.parseInt(placeId));

		return myString;
	}

	/**
	 * Metoda zwracajaca punkty znajdujace sie w zadanym promieniu od podanego
	 * punktu.
	 * 
	 * @param lon
	 *            dlugosc geograficzna punktu
	 * @param lat
	 *            szerokosc geograficzna punktu
	 * @param range
	 *            promien w metrach
	 * @return Liste miejsc w formacie JSON
	 * @throws Exception
	 *             w przypadku nieudanego odczytu z bazy danych
	 */
	@Path("/get/{lon}/{lat}/{range}")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getPointsInRange(@PathParam("lon") String lon, @PathParam("lat") String lat,
			@PathParam("range") String range) throws Exception {

		logger.debug(" Received request for places ");
		logger.debug(lon + " " + lat + " " + range);

		PostgresDB dao = new PostgresDB();
		String myString = dao.getPointsWithinRange(lon, lat, range);

		if (myString == null)
			logger.debug(" Failed to get points from DB");
		else
			logger.debug(myString);

		return myString;
	}
	
	/**
	 * Metoda zwracajaca wszystkie miejsca z poczekalni
	 * 
	 * @return lista miejsc w formacie JSON
	 * @throws Exception
	 */
	@Path("/get/all")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getAllPoints() throws Exception {
		PostgresDB dao = new PostgresDB();
		String myString = dao.getAllPointsFromWaitingRoom();

		return myString;
	}

	/**
	 * Metoda usuwajaca dane miejsce (oraz powiazane zdjecie i miniature)
	 * 
	 * @param hash skrot SHA powiazany z miejscem
	 * @return status wykonania operacji, 200 - OK, 500 - ERROR
	 * @throws Exception
	 */
	@Path("/delete/waiting/{hash}")
	@DELETE
	@Produces(MediaType.TEXT_PLAIN)
	public String deletePlaceFromWaitingRoom(@PathParam("hash")String hash) throws Exception {
		logger.debug(" Received delete request "+hash);
		
		PostgresDB dao = new PostgresDB();
		boolean result = dao.deletePlaceFromWaitingRoom(hash);
		if(result)
			return "200";
		else
			return "500";
	}
	
	/**
	 * Pobiera wszystkie miejsca o danej kategori
	 * 
	 * @param category szukana kategoria
	 * @return lista miejsc w formacie JSON
	 * @throws Exception
	 */
	@Path("/get/category/{category}")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getPlacesWithSpecifiedCategory(@PathParam("category") String category) throws Exception{
		PostgresDB dao = new PostgresDB();
		String json = dao.getPlacesWithCategory(category);
		return json;
	}
	
	/**
	 * Metoda pobiera wszystkie dostepne w bazie kategorie
	 * 
	 * @return lista kategori w formacie JSON
	 */
	@Path("/get/categories")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getPlacesCategories(){
		PostgresDB dao = new PostgresDB();
		String json = dao.getAllPlacesCategories();
		return json;
	}
	
}