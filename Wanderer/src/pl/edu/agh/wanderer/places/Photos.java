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
	
	@Path("/get/{id}")
	@GET
	@Produces({"image/jpeg"})
	public Response getPlaceDesc(@PathParam("id") String photoId) throws Exception {
		
		PostgresDB dao = new PostgresDB();
		byte [] result = dao.getPhoto(Integer.parseInt(photoId));
		return Response.ok(result).build();
		
		
	}
	
	@Path("/get/thumbnail/{id}")
	@GET
	@Produces({"image/jpeg"})
	public Response getPlaceThumbnail(@PathParam("id") String photoId) throws Exception {
		
		
		PostgresDB dao = new PostgresDB();
		byte [] result = dao.getThumbnail(Integer.parseInt(photoId));
		System.out.println(" sending photo, bytes: "+result.length);
		return Response.ok(result).build();
		
		
	}
	
	@Path("/set/photo/{id}")
	@POST
	@Consumes("image/jpeg")
	@Produces(MediaType.TEXT_PLAIN)
	public String setPlacePhoto(@PathParam("id") String photoId, byte [] incomingData) throws Exception {
		
		System.out.println(" photo id "+photoId);
		FileOutputStream fos = new FileOutputStream("D:/img/recv.jpg");
		fos.write(incomingData);
		fos.close();
		return "500";
		
		
	}
	
	@Path("/get/meta/{id}")
	@GET
	@Produces(MediaType.TEXT_PLAIN)
	public String getPhotoMetadata(@PathParam("id") String id) throws Exception {

		System.out.println(" received message ");
		PostgresDB dao = new PostgresDB();
		String myString = dao.getPhotoMetadata(Integer.parseInt(id));

		return myString;
	}
}
