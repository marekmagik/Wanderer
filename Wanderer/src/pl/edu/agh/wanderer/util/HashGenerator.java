package pl.edu.agh.wanderer.util;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Formatter;

public class HashGenerator {

	public static String generateHash(ByteArrayInputStream imageInputStream) throws NoSuchAlgorithmException, IOException {
		byte[] bFile = new byte[imageInputStream.available()];

		imageInputStream.read(bFile);
		imageInputStream.reset();
		imageInputStream.close();

		MessageDigest md = MessageDigest.getInstance("SHA-256");
		md.update(bFile);
		byte[] hash = md.digest();

		return byteArray2Hex(hash);
	}

	private static String byteArray2Hex(final byte[] hash) {
		Formatter formatter = new Formatter();
		
		for (byte b : hash) {
			formatter.format("%02x", b);
		}

		String result = formatter.toString();
		formatter.close();
		return result;
	}
}
