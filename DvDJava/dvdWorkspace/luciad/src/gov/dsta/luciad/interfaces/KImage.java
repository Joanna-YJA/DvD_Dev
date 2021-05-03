package gov.dsta.luciad.interfaces;

import java.awt.Image;
import java.util.HashMap;

public class KImage extends KGraphic {

	private static final String CLASS_IDENTIFIER = "Image";
	
	private String _filename = "";
//	private int _width = 0;
//	private int _height = 0;
	private double _degree = 0;
	private double pitch = 0;
	private double roll = 0;
	private Image selectedImg;
	private Image image;
	private Image reduceDisplayImg;
	private String image3DPath;
	private double scale =1;
	private boolean shouldPlotOnMiniMap = false;
	private boolean isRed = false;
	private double screenX = 0;
	private double screenY = 0;
	String id;
	public void setId(String id) {
		this.id = id;
	}

	//tcau
	private boolean useOrientation = false;
	private Object customObject = null;
	private HashMap<String, Object> properties = new HashMap<String, Object>();
	IKPoint _spatial;
	public HashMap<String, Object> getProperties() {
		return properties;
	}

	public KImage() {
		this(null, "", 0, 0, 0.0f);
	}
	
	public KImage(IKPoint point, String filename, int width, int height, double degree)
	{
		_spatial = point;
		_filename = filename;
//		_width = width;
//		_height = height;
		_degree = degree;
	}
	
	

	public void setProperties(HashMap<String, Object> properties) {
		this.properties = properties;
	}

	public double getScreenX() {
		return screenX;
	}

	public void setScreenX(double screenX) {
		this.screenX = screenX;
	}

	public double getScreenY() {
		return screenY;
	}

	public void setScreenY(double screenY) {
		this.screenY = screenY;
	}

	public Object getCustomObject() {
		return customObject;
	}

	public void setCustomObject(Object customObject) {
		this.customObject = customObject;
	}


	
	public void setLocation(IKPoint point) {
		_spatial = point;
	}
	
	public IKPoint getLocation() {
		return (IKPoint)_spatial;
	}

	public String getFilename() {
		return _filename;
	}

	public void setFilename(String filename) {
		_filename = filename;
	}
	public Image getSelectedImg() {
		return selectedImg;
	}

	public void setSelectedImg(Image selectedImg) {
		this.selectedImg = selectedImg;
	}

	public Image getImage() {
		return image;
	}

	public void setImage(Image image) {
		this.image = image;
	}
	public Image getReduceDisplayImg() {
		return reduceDisplayImg;
	}

	public void setReduceDisplayImg(Image reduceDisplayImg) {
		this.reduceDisplayImg = reduceDisplayImg;
	}
	
	//tcau - added getter/setter to support icon rotation
	public void setOrientation(double angleInDegree){
		_degree = angleInDegree;
	}
	
	public double getOrientation(){
		return _degree;
	}

	/**
	 * @return the useOrientation
	 */
	public boolean isUseOrientation() {
		return useOrientation;
	}

	/**
	 * @param useOrientation the useOrientation to set
	 */
	public void setUseOrientation(boolean useOrientation) {
		this.useOrientation = useOrientation;
	}

	public String getImage3DPath() {
		return image3DPath;
	}

	public void setImage3DPath(String image3dPath) {
		image3DPath = image3dPath;
	}

	public double getScale() {
		return scale;
	}

	public void setScale(double scale) {
		this.scale = scale;
	}

	public double getPitch() {
		return pitch;
	}

	public void setPitch(double pitch) {
		this.pitch = pitch;
	}

	public double getRoll() {
		return roll;
	}

	public void setRoll(double roll) {
		this.roll = roll;
	}

	public boolean isShouldPlotOnMiniMap() {
		return shouldPlotOnMiniMap;
	}

	public void setShouldPlotOnMiniMap(boolean shouldPlotOnMiniMap) {
		this.shouldPlotOnMiniMap = shouldPlotOnMiniMap;
	}
	
	public boolean isRed() {
		return isRed;
	}
	
	public void setIsRed(boolean isRed) {
		this.isRed = isRed;
	}

	public String getId() {
		// TODO Auto-generated method stub
		return null;
	}
	
}
