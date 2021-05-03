/*
 * 
 * Java sample codes for demonstrating how to use Tensorflow for Java object detection API
 * 
 * @author Huang Tingxing
 * @version 1.0, September 9, 2017
 */

package gov.dsta.vca.udp;

import java.io.ByteArrayInputStream;
import java.io.DataInputStream;

public class DetectionMessage extends Command{
	private static final int _255 = 255;
	private String name = "";
	private float score = 1.0f;
	private int minX = 0;
	private int minY = 0;
	private int maxX = 0;
	private int maxY = 0;
	private int width = 0;
	private int height = 0;
	private int red = _255;
	private int green = _255;
	private int blue = 0;
	private long lastUpdateTime = System.currentTimeMillis();
	private double scale = 0;
	
	
	public DetectionMessage() {
		
	}


	public DetectionMessage(String name, float score, int minX, int minY, int maxX, int maxY) {
		this.name = name;
		this.score = score;
		this.minX = minX;
		this.minY = minY;
		this.maxX = maxX;
		this.maxY = maxY;
	}
	
	public DetectionMessage(String name, float score, int minX, int minY, int maxX, int maxY, int red, int green, int blue) {
		this.name = name;
		this.score = score;
		this.minX = minX;
		this.minY = minY;
		this.maxX = maxX;
		this.maxY = maxY;
		
		this.red = red;
		this.green = green;
		this.blue = blue;
	}
	
	public String getName() {
		return name;
	}
	
	public void setName(String name) {
		this.name = name;
	}
	
	public float getScore() {
		return score;
	}
	
	public void setScore(float score) {
		this.score = score;
	}
	
	public int getMinX() {
		return minX;
	}
	
	public void setMinX(int minX) {
		this.minX = minX;
	}
	
	public int getMinY() {
		return minY;
	}
	
	public void setMinY(int minY) {
		this.minY = minY;
	}
	
	public int getMaxX() {
		return maxX;
	}
	
	public void setMaxX(int maxX) {
		this.maxX = maxX;
	}

	public int getMaxY() {
		return maxY;
	}
	
	public void setMaxY(int maxY) {
		this.maxY = maxY;
	}
	
	public int getRed() {
		return red;
	}
	
	public void setRed(int red) {
		this.red = red;
	}

	public int getGreen() {
		return green;
	}
	
	public void setGreen(int green) {
		this.green = green;
	}
	
	public int getBlue() {
		return blue;
	}
	
	public void setBlue(int blue) {
		this.blue = blue;
	}
	
	public long getLastUpdateTime() {
		return lastUpdateTime;
	}

	public void setLastUpdateTime(long lastUpdateTime) {
		this.lastUpdateTime = lastUpdateTime;
	}
	
	public int getWidth() {
		return width;
	}

	public void setWidth(int width) {
		this.width = width;
	}

	public int getHeight() {
		return height;
	}

	public void setHeight(int height) {
		this.height = height;
	}

	@Override
	public byte[] getData() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void readData(byte[] data) {
		ByteArrayInputStream bytearrypacket = new ByteArrayInputStream(data);
		DataInputStream dataInputpacket = new DataInputStream(bytearrypacket);
		try{
			name = "Drone";
			
			minX = dataInputpacket.readInt();
			minY = dataInputpacket.readInt();
			width = dataInputpacket.readInt();
			height = dataInputpacket.readInt();

		}catch(Exception e){
			e.printStackTrace();
		}
		

		
	}

	@Override
	public String getTopic() {
		// TODO Auto-generated method stub
		return null;
	}

}
