package gov.dsta.vca;

import java.awt.Color;

public class LabelItem implements IItem {
	
	private static final int _255 = 255;
	private String name = "";
	private int id = 0;
	private String label = "";
	private int red = _255;
	private int green = _255;
	private int blue = 0;
	private int alpha = _255;
	
	public LabelItem() {
	}
	
	public LabelItem(String name, int id, String label) {
		this.name = name;
		this.id = id;
		this.label = label;
	}
	
	public LabelItem(String name, int id, String label, int red, int green, int blue) {
		this.name = name;
		this.id = id;
		this.label = label;
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
	
	public int getId() {
		return id;
	}
	
	public void setId(int id) {
		this.id = id;
	}
	
	public String getLabel() {
		return label;
	}
	
	public void setLabel(String label) {
		this.label = label;
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
	
	public int getAlpha() {
		return alpha;
	}
	
	public void setAlpha(int alpha) {
		this.alpha = alpha;
	}
	
	public void setRgb(int red, int green, int blue) {
		this.red = red;
		this.green = green;
		this.blue = blue;
	}
	
	public int getRgb() {
		return toColor().getRGB();
	}
	
	public Color toColor() {
		return new Color(red, green, blue, alpha);
	}

}
