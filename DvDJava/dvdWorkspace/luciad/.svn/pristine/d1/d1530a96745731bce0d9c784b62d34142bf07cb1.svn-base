package gov.dsta.luciad;

import com.luciad.shape.shape2D.TLcdLonLatPoint;

public class LonLatPointModel extends TLcdLonLatPoint {

	protected Long id;
	private static Long count = 0L;
	
//	public LonLatPointModel(double longitude, double latitude, Long id) {
//		super(longitude,latitude);
//		this.id = id;
//	}
	public LonLatPointModel() {
		super();
		this.id = count;
		count++;
	}
	
	public LonLatPointModel(double longitude, double latitude) {
		super(longitude,latitude);
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
		return  id.hashCode();
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (!super.equals(obj))
			return false;
		if (getClass() != obj.getClass())
			return false;
		LonLatPointModel other = (LonLatPointModel) obj;
		if (id == null) {
			if (other.id != null)
				return false;
		} else if (!id.equals(other.id))
			return false;
		return true;
	}
	
	
}
