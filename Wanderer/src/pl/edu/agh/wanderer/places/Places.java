package pl.edu.agh.wanderer.places;

import javax.ws.rs.DELETE;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;

import pl.edu.agh.wanderer.dao.PostgresDB;

@Path("/places")
public class Places {

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

		System.out.println(" Sending description for place with id " + placeId);
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

		System.out.println(" Received request for places ");
		System.out.println(lon + " " + lat + " " + range);

		PostgresDB dao = new PostgresDB();
		String myString = dao.getPointsWithinRange(lon, lat, range);

		if (myString == null)
			System.out.println(" Failed to get points from DB");
		else
			System.out.println(myString);

		return myString;
	}
	
	@Path("/get/all")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getAllPoints() throws Exception {
		PostgresDB dao = new PostgresDB();
		String myString = dao.getAllPointsFromWaitingRoom();

		return myString;
	}

	@Path("/delete/waiting/{hash}")
	@DELETE
	@Produces(MediaType.TEXT_PLAIN)
	public String deletePlaceFromWaitingRoom(@PathParam("hash")String hash) throws Exception {
		System.out.println(" Received delete request "+hash);
		
		PostgresDB dao = new PostgresDB();
		boolean result = dao.deletePlaceFromWaitingRoom(hash);
		if(result)
			return "200";
		else
			return "500";
	}
	
	@Path("/get/category/{category}")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getPlacesWithSpecifiedCategory(@PathParam("category") String category) throws Exception{
		PostgresDB dao = new PostgresDB();
		String json = dao.getPlacesWithCategory(category);
		return json;
	}
	
	@Path("/get/categories")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getPlacesCategories(){
		PostgresDB dao = new PostgresDB();
		String json = dao.getAllPlacesCategories();
		return json;
	}
	
}