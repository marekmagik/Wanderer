package pl.edu.agh.wanderer.places;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;

import pl.edu.agh.wanderer.dao.PostgresDB;

@Path("/places")
public class Places {

	@Path("/get/{id}")
	@GET
	@Produces(MediaType.TEXT_PLAIN)
	public String getPlaceDesc(@PathParam("id") String id) throws Exception {

		System.out.println(" sending description for "+id);
		PostgresDB dao = new PostgresDB();
		String myString = dao.getPlaceDesc(Integer.parseInt(id));

		return myString;
	}

	@Path("/get/{lon}/{lat}/{range}")
	@GET
	@Produces(MediaType.APPLICATION_JSON)
	public String getPointsInRange(@PathParam("lon") String lon,
			@PathParam("lat") String lat, @PathParam("range") String range)
			throws Exception {
		System.out.println(" received message ");
		System.out.println(lon+" "+lat+" "+range);
		PostgresDB dao = new PostgresDB();
		String myString = dao.getPointsWithinRange(lon, lat, range);
		
		if(myString==null)
			System.out.println("null ;/");
		else
			System.out.println(myString);
			
		
		return myString;
	}

}