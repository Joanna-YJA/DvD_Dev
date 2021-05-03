package gov.dsta.dvd.dif.video.util;

public class FOVUtil {

	public static double convertHFOVToVFOV(double hFOV , double width, double height){
		double vFOV = 2.0 * Math.atan(height/width * Math.tan(Math.toRadians(hFOV)/2.0));
		return Math.toDegrees(vFOV);
		
	}
	
	public static double convertVFOVToHFOV(double vFOV, double width, double height){
		double hFOV = 2 * Math.atan(Math.tan(Math.toRadians(vFOV)/2)/(height/width));
		return Math.toDegrees(hFOV);
		
	}
}
