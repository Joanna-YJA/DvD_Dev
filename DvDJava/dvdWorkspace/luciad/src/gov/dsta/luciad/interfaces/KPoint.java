package gov.dsta.luciad.interfaces;


public class KPoint implements IKPoint {
	
	protected double _x = 0;
	protected double _y = 0;
	protected double _z = 0;
	
	public KPoint() {
		this(0, 0, 0);
	}
	
	public KPoint(double x, double y) {
		this(x, y, 0);
	}
	
	public KPoint(double x, double y, double z) {
		_x = x;
		_y = y;
		_z = z;
	}


	
	@Override
	public double getX() {
		// TODO Auto-generated method stub
		return _x;
	}

	@Override
	public void setX(double x) {
		// TODO Auto-generated method stub
		_x = x;
	}

	@Override
	public double getY() {
		// TODO Auto-generated method stub
		return _y;
	}

	@Override
	public void setY(double y) {
		// TODO Auto-generated method stub
		_y = y;
	}

	@Override
	public double getZ() {
		// TODO Auto-generated method stub
		return _z;
	}

	@Override
	public void setZ(double z) {
		// TODO Auto-generated method stub
		_z = z;
	}
	


	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		long temp;
		temp = Double.doubleToLongBits(_x);
		Long longResult = (temp ^ (temp >>> 32));
		result = prime * result + longResult.intValue();
		temp = Double.doubleToLongBits(_y);
		longResult = (temp ^ (temp >>> 32));
		result = prime * result + longResult.intValue();
		temp = Double.doubleToLongBits(_z);
		longResult = (temp ^ (temp >>> 32));
		result = prime * result + longResult.intValue();
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		KPoint other = (KPoint) obj;
		if (Double.doubleToLongBits(_x) != Double.doubleToLongBits(other._x))
			return false;
		if (Double.doubleToLongBits(_y) != Double.doubleToLongBits(other._y))
			return false;
		if (Double.doubleToLongBits(_z) != Double.doubleToLongBits(other._z))
			return false;
		return true;
	}

	@Override
	public IKPoint toScreen(double scale, IKPoint basePoint) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void copy(IKPoint src) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void move(double dx, double dy) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public double getXInMetre() {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public double getYInMetre() {
		// TODO Auto-generated method stub
		return 0;
	}
	
}
