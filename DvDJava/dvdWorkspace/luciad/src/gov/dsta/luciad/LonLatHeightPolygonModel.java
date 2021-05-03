package gov.dsta.luciad;

import com.luciad.geodesy.ILcdEllipsoid;
//import com.luciad.shape.ILcdPoint;
//import com.luciad.shape.shape2D.ILcd2DEditablePointList;
//import com.luciad.shape.shape2D.TLcdLonLatPolygon;
import com.luciad.shape.shape3D.ILcd3DEditablePointList;
import com.luciad.shape.shape3D.TLcdLonLatHeightPolygon;

public class LonLatHeightPolygonModel extends TLcdLonLatHeightPolygon{
	private Long id;
	private static Long count = 0L;
	
//	public LonLatPointModel(double longitude, double latitude, Long id) {
//		super(longitude,latitude);
//		this.id = id;
//	}
	public LonLatHeightPolygonModel() {
		super();
		this.id = count;
		count++;	
	}

	public LonLatHeightPolygonModel(ILcd3DEditablePointList pointList, ILcdEllipsoid ellipsoid) {
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
		if(!(obj instanceof LonLatHeightPolygonModel)){
			return false;
		}
		LonLatHeightPolygonModel other = (LonLatHeightPolygonModel) obj;
		if (id == null) {
			if (other.id != null)
				return false;
		} else if (!id.equals(other.id))
			return false;
		return true;
	}

	

	@Override
	public void set3DEditablePointList(ILcd3DEditablePointList a3dEditablePointList) {
		// TODO Auto-generated method stub
		super.set3DEditablePointList(a3dEditablePointList);
	}
	
	public ILcd3DEditablePointList get3DEditablePointList2(){
		return super.get3DEditablePointList();
	}
	

	
}
