package gov.dsta.luciad;

import java.awt.Color;

import com.luciad.shape.shape3D.TLcdLonLatHeightPoint;

public class LonLatHeightPointModel extends TLcdLonLatHeightPoint {

	protected Long id;
	private static Long count = 0L;
	private String annotationId = "";
	private Color color;
@Override
public boolean equals(Object obj) {
	
	if (getClass() != obj.getClass())
		return false;
	LonLatHeightPointModel other = (LonLatHeightPointModel) obj;
	if (annotationId == null) {
		if (other.annotationId != null)
			return false;
	} else if (!annotationId.equals(other.annotationId))
		return false;
	if (id == null) {
		if (other.id != null)
			return false;
	} else if (!id.equals(other.id))
		return false;
	return true;
}

	//	public LonLatPointModel(double longitude, double latitude, Long id) {
//		super(longitude,latitude);
//		this.id = id;
//	}
	public LonLatHeightPointModel() {
		super();
		this.id = count;
		count++;
	}
	
	public LonLatHeightPointModel(double longitude, double latitude, double height) {
		super(longitude,latitude,height);
		this.id = count;
		count++;
	}
	public Long getId() {
		return id;
	}

	public void setId(Long id) {
		this.id = id;
	}

	@Override
	public int hashCode() {
		return id.hashCode();
	}

	public String getAnnotationId() {
		return annotationId;
	}

	public void setAnnotationId(String annotationId) {
		this.annotationId = annotationId;
	}

	public Color getColor() {
		return color;
	}

	public void setColor(Color color) {
		this.color = color;
	}

	
	
	
}
