package gov.dsta.dvd.dif.video.util;

import java.awt.image.BufferedImage;
import java.io.File;

import javax.imageio.ImageIO;

public class ImageConstant {

	
	public static BufferedImage RED_ARROW = loadImage("resource/image/Arrow.png");
	public static BufferedImage DRONE = loadImage("resource/image/drone.png");


	private static BufferedImage loadImage(String path) {
		BufferedImage img = null;

		try {
			img = ImageIO.read(new File(path));

		} catch (Exception e) {
			e.printStackTrace();
		}

		return img;
	}


}
