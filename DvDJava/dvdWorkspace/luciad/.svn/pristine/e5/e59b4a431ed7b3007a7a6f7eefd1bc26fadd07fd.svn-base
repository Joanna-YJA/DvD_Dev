package gov.dsta.luciad;

import com.luciad.geodesy.ILcdEllipsoid;
import com.luciad.shape.shape2D.ILcd2DEditablePointList;
import com.luciad.shape.shape2D.TLcdLonLatPolygon;

public class LonLatPolygonModel extends TLcdLonLatPolygon{
	private Long id;
	private static Long count = 0L;
	
//	public LonLatPointModel(double longitude, double latitude, Long id) {
//		super(longitude,latitude);
//		this.id = id;
//	}
	public LonLatPolygonModel() {
		super();
		this.id = count;
		count++;	
	}

	public LonLatPolygonModel(ILcd2DEditablePointList pointList, ILcdEllipsoid ellipsoid) {
		super(pointList,ellipsoid );
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

	@Override
	public boolean equals(Object obj) {
		if(!(obj instanceof LonLatPolygonModel)){
			return false;
		}
		LonLatPolygonModel other = (LonLatPolygonModel) obj;
		if (id == null) {
			if (other.id != null)
				return false;
		} else if (!id.equals(other.id))
			return false;
		return true;
	}

	
}
