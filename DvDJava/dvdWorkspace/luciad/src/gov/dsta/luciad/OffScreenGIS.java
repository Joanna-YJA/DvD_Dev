package gov.dsta.luciad;

import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.Rectangle;
import java.awt.Toolkit;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.List;

import javax.swing.JFrame;
import javax.swing.JPanel;

import com.luciad.format.raster.TLcdGeoTIFFModelDecoder;
import com.luciad.geodesy.TLcdGeodeticDatum;
import com.luciad.gui.TLcdImageIcon;
import com.luciad.model.ILcdModel;
import com.luciad.model.TLcdVectorModel;
import com.luciad.reference.ILcdGeoReference;
import com.luciad.reference.TLcdGeodeticReference;
import com.luciad.shape.ILcdBounds;
import com.luciad.shape.ILcdPoint;
import com.luciad.shape.shape2D.TLcdXYBounds;
import com.luciad.shape.shape3D.TLcdLonLatHeightPoint;
import com.luciad.shape.shape3D.TLcdXYZPoint;
import com.luciad.view.lightspeed.ILspView;
import com.luciad.view.lightspeed.TLspAWTView;
import com.luciad.view.lightspeed.TLspContext;
import com.luciad.view.lightspeed.TLspViewBuilder;
import com.luciad.view.lightspeed.camera.ALspCameraConstraint;
import com.luciad.view.lightspeed.camera.TLspViewXYZWorldTransformation3D;
import com.luciad.view.lightspeed.camera.aboveterrain.TLspAboveTerrainCameraConstraint3D;
import com.luciad.view.lightspeed.layer.ILspInteractivePaintableLayer;
import com.luciad.view.lightspeed.layer.ILspLayer;
import com.luciad.view.lightspeed.layer.TLspPaintRepresentationState;
import com.luciad.view.lightspeed.layer.TLspPaintState;
import com.luciad.view.lightspeed.layer.raster.TLspRasterLayerBuilder;
import com.luciad.view.lightspeed.layer.shape.TLspShapeLayerBuilder;
import com.luciad.view.lightspeed.query.TLspPaintedObjectsQuery;
import com.luciad.view.lightspeed.style.ALspStyle;
import com.luciad.view.lightspeed.style.ILspWorldElevationStyle.ElevationMode;
import com.luciad.view.lightspeed.style.TLsp3DIconStyle;
import com.luciad.view.lightspeed.style.TLspIconStyle;
import com.luciad.view.lightspeed.style.TLspIconStyle.ScalingMode;
import com.luciad.view.lightspeed.style.styler.TLspEditableStyler;

import gov.dsta.luciad.interfaces.IKGraphic;
import gov.dsta.luciad.interfaces.KImage;
import gov.dsta.luciad.interfaces.KPoint;

public class OffScreenGIS {
	private static final String MSG_PREFIX = "@OffScreenGIS";
	private static final int _60 = 60;
	private static final double _30_0 = 30.0;
	public final static JFrame frame = new JFrame();
	private static final double MAP_CENTER_X = 103.687590;
	private static final double MAP_CENTER_Y = 1.391416;
	private static final double MAP_CENTER_Z = 53.370651;
	private static double mapRotation = 0.0;
	private static double mapPitch = -_30_0;
	public static TLspAWTView view;
	private JPanel mapPanel;
	private static Dimension dim = Toolkit.getDefaultToolkit().getScreenSize();
	private static Double screenWidth = dim.getWidth();
	private static Double screenHeight = dim.getHeight();
//	private ILspLayer mapLayer = null;
	private static TLcdGeoTIFFModelDecoder decoder = new TLcdGeoTIFFModelDecoder();
	public static HashMap<Object, IKGraphic> arLabelHashMap = new HashMap<>();
	public static HashMap<IKGraphic, Object> arLuciadShapeHashMap = new HashMap<>();

//	private RasterStyler elevationStyler = new RasterStyler(
//			TLspRasterStyle.newBuilder()//.startResolutionFactor(RESOLUTION_FACTOR)
//			.opacity(1)
//			.colorMap(TLcdColorModelFactory.createElevationColorMap()).build());
	public OffScreenGIS(){
		view =TLspViewBuilder.newBuilder().addAtmosphere(false)
		.viewType(ILspView.ViewType.VIEW_3D).buildAWTView();
		//view.setController(null);
		//ZClear zclear =  new ZClear();

		try{
			//ILcdModel model = loadLocalMap("map/103wgs84_1sec.dt2");
		ILcdModel model = loadLocalMap("resource/map/topo/Project_50mr11.tif");
			ILspLayer mapLayer = null;
//			ILcdModelDescriptor modelDescriptor = model.getModelDescriptor();
		//	if (modelDescriptor instanceof TLcdDTEDModelDescriptor) {
				// DTEDModel
//				System.err.println("creating offscreen map");
				mapLayer = TLspRasterLayerBuilder.newBuilder().model(model).layerType(ILspLayer.LayerType.BACKGROUND)
						//.styler(TLspPaintRepresentationState.REGULAR_BODY, elevationStyler)
						.build();
//				
				view.addLayer(mapLayer);
				//view.addLayer(zclear);
				//zclear.setVisible(true);
				camera = (TLspViewXYZWorldTransformation3D) view
						.getViewXYZWorldTransformation();
				createDrawingLayer();
				mapPanel = new JPanel();
				mapPanel.setLayout(new BorderLayout());
				mapPanel.add(view.getHostComponent(), BorderLayout.CENTER);
				mapPanel.validate();
				frame.getContentPane().setLayout(new BorderLayout());
                frame.setUndecorated(true);
                frame.validate();
                frame.setLocationRelativeTo(null);
                frame.setVisible(true);
                frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
                
               

                frame.setBounds(0, 0, screenWidth.intValue(), screenHeight.intValue());
				frame.add(view.getHostComponent());
				centerMap(MAP_CENTER_X,MAP_CENTER_Y,MAP_CENTER_Z, mapRotation, mapPitch, 0, _60);
				frame.setVisible(false);
                
                TLspViewXYZWorldTransformation3D camera = (TLspViewXYZWorldTransformation3D) view
    					.getViewXYZWorldTransformation();
    			for(ALspCameraConstraint<TLspViewXYZWorldTransformation3D> o : camera.getConstraints()){
    				if(o instanceof TLspAboveTerrainCameraConstraint3D){
    					((TLspAboveTerrainCameraConstraint3D<?>) o).setMinAltitude(0);

    				}
    			}

		//	}
			
		}catch(Exception e){
			e.printStackTrace();
//			e.printStackTrace();
		}
	}
	private static TLspViewXYZWorldTransformation3D camera;
	private static ILspInteractivePaintableLayer drawingLayer;
	public static void centerMap(double x, double y , double z , double yaw, double pitch, double roll, double fov){
		TLcdXYZPoint eye = new TLcdXYZPoint(
				x,
				y,
				z);
		
//		TLspViewXYZWorldTransformation3D camera = (TLspViewXYZWorldTransformation3D) view
//				.getViewXYZWorldTransformation();
//		System.err.println("roll" + roll +"pitch" + pitch);

		camera.lookFrom(EulerAngleUtil.getXYZ(eye, (ILcdGeoReference) view.getXYZWorldReference()),
				0,
				yaw,
				pitch,
				roll);

		camera.setFieldOfView(fov);
	}
	public KImage getCurrentPosition(){
		KImage kPoint = new KImage();
		kPoint.setLocation(new KPoint());
		ILcdPoint point = EulerAngleUtil.getLatLonHeight(camera.getEyePoint(), (ILcdGeoReference) view.getXYZWorldReference());
		kPoint.getLocation().setX(point.getX());
		kPoint.getLocation().setY(point.getY());
		kPoint.getLocation().setZ(point.getZ());
		kPoint.setOrientation(camera.getYaw());
		return kPoint;

	}
	public static List<KImage> getAllObject() {

        HashMap<String, KImage> screenXYResultMap = new HashMap<String, KImage>();
//        List<KImage> screenXYResult = new ArrayList<>();
        for(Object obj : arLuciadShapeHashMap.values()){
        	if(obj instanceof TLcdLonLatHeightPoint){
        		KImage kGraphic = (KImage) arLabelHashMap.get(obj);
        		if(kGraphic != null){
        			TLcdXYZPoint screenXY = transformWorldToView((TLcdLonLatHeightPoint) obj);
            		KImage kModel = new KImage();
            		kModel.setIsRed(kGraphic.isRed());
            		kModel.setId(kGraphic.getId());
            		kModel.setScreenX(screenXY.getX());
            		kModel.setScreenY(screenXY.getY());
            	    kModel.setLocation(new KPoint(((TLcdLonLatHeightPoint) obj).getX(), ((TLcdLonLatHeightPoint) obj).getY()));
            	    kModel.setImage(kGraphic.getImage());
            	    kModel.setProperties(kGraphic.getProperties());
            		screenXYResultMap.putIfAbsent(kModel.getId(), kModel);
        		}else{
    				System.err.println("OFF SCREEN MISSING OBJ");
        		}
        		
        		
        	}
        }
       ;
        return  new ArrayList<>(screenXYResultMap.values());
	}
	public static List<KImage> getAllOnScreenObject(){
		ILcdBounds rectangle = new TLcdXYBounds(
		        0,
		        0,
		        screenWidth,
		        screenHeight
		    );
	    TLspContext context = new TLspContext();
	     context.resetFor(drawingLayer, view);

	    TLspPaintedObjectsQuery query = new TLspPaintedObjectsQuery(TLspPaintRepresentationState.REGULAR_BODY, rectangle);
        Collection<Object> result = drawingLayer.query(query, context);
        HashMap<String, KImage> screenXYResultMap = new HashMap<String, KImage>();
//        List<KImage> screenXYResult = new ArrayList<>();
        for(Object obj : result){
        	if(obj instanceof TLcdLonLatHeightPoint){
        		KImage kGraphic = (KImage) arLabelHashMap.get(obj);
        		if(kGraphic != null){
        			TLcdXYZPoint screenXY = transformWorldToView((TLcdLonLatHeightPoint) obj);
            		KImage kModel = new KImage();
            		kModel.setIsRed(kGraphic.isRed());
            		kModel.setId(kGraphic.getId());
//					System.err.println("DRAWING lable at " + screenXY.getX() + ", " + screenXY.getY() + ". " + " , " + screenXY.getZ() );
            		kModel.setScreenX(screenXY.getX());
            		kModel.setScreenY(screenXY.getY());
            	    kModel.setLocation(new KPoint(((TLcdLonLatHeightPoint) obj).getX(), ((TLcdLonLatHeightPoint) obj).getY()));
            	    kModel.setImage(kGraphic.getImage());
            	    kModel.setProperties(kGraphic.getProperties());
//            		screenXYResult.add(kModel);
            		screenXYResultMap.putIfAbsent(kModel.getId(), kModel);
        		}else{
    				System.err.println("OFF SCREEN MISSING OBJ");
        		}
        		
        		
        	}
        }
       ;
        return  new ArrayList<>(screenXYResultMap.values());
	}
	private static TLcdXYZPoint transformWorldToView(TLcdLonLatHeightPoint point){
		TLcdXYZPoint pointSFCT = new TLcdXYZPoint();
		TLcdLonLatHeightPoint tempPt = new TLcdLonLatHeightPoint(point.getX(),point.getY(),point.getZ());
		camera.worldPoint2ViewSFCT(EulerAngleUtil.getXYZ(tempPt, (ILcdGeoReference) view.getXYZWorldReference()), pointSFCT);
//		camera.worldPoint2ViewSFCT(EulerAngleUtil.getXYZ(point, (ILcdGeoReference) view.getXYZWorldReference()), pointSFCT);

		return pointSFCT;
	}
	


	private static void createDrawingLayer(){
		TLcdVectorModel model = new TLcdVectorModel();
		TLcdGeodeticDatum datum = new TLcdGeodeticDatum();
		model.setModelReference(new TLcdGeodeticReference(datum));
		TLspShapeLayerBuilder layerBuilder = TLspShapeLayerBuilder.newBuilder();
		drawingLayer = layerBuilder.model(model).selectable(true).bodyEditable(false)
				.bodyStyler(TLspPaintState.REGULAR, new TLspEditableStyler()).culling(false)
				.bodyStyler(TLspPaintState.SELECTED, new TLspEditableStyler())
				.build();
		view.addLayer(drawingLayer);
	}
	public static void removeIcon(KImage imgModel){
		if(!arLuciadShapeHashMap.containsKey(imgModel)){
			System.err.println("OFFSCREEN NOT FOUND DELETE!");
			return;
		}

		OrientationLonLatHeightPointModel imageShape = (OrientationLonLatHeightPointModel) arLuciadShapeHashMap.get(imgModel);
		arLabelHashMap.remove(imageShape);
		arLuciadShapeHashMap.remove(imgModel);
		drawingLayer.getModel().removeElement(imageShape, ILcdModel.FIRE_NOW);
	}
	public static void plotIcon(KImage imgModel){
			ALspStyle iconStyle =null;
			if(imgModel.getImage3DPath() != null){
				iconStyle = TLsp3DIconStyle.newBuilder()
		                .icon(imgModel.getImage3DPath())
		                .scale(1)
		                .iconSizeMode(TLsp3DIconStyle.ScalingMode.SCALE_FACTOR).elevationMode(ElevationMode.OBJECT_DEPENDENT).recenterIcon(false)
		                .build();
				
			}else{
				iconStyle = TLspIconStyle.newBuilder().icon(new TLcdImageIcon(imgModel.getImage()))
						// Set icons to have fixed view coordinates
						// .scalingMode(ScalingMode.VIEW_SCALING)
						.scalingMode(ScalingMode.WORLD_SCALING_CLAMPED).useOrientation(imgModel.isUseOrientation()).scale(1.0)
						.elevationMode(ElevationMode.OBJECT_DEPENDENT)
						// Set the icons' alpha value
						.opacity(1.0f).build();
			}
		 
		 OrientationLonLatHeightPointModel imageShape = new OrientationLonLatHeightPointModel(imgModel.getLocation().getX(),
				 imgModel.getLocation().getY(),imgModel.getLocation().getZ());
			double aOrientation = imgModel.getOrientation();

			imageShape.setOrientation(aOrientation);
			imageShape.setPitch(imgModel.getPitch());
			imageShape.setRoll(imgModel.getRoll());
		 TLspEditableStyler mainStyler = (TLspEditableStyler) ((ILspInteractivePaintableLayer) drawingLayer)
					.getStyler(TLspPaintRepresentationState.REGULAR_BODY);
		 mainStyler.setStyle(drawingLayer.getModel(), imageShape,
					Arrays.<com.luciad.view.lightspeed.style.ALspStyle> asList(iconStyle));

			drawingLayer.getModel().addElement(imageShape, ILcdModel.FIRE_NOW );
			arLabelHashMap.put(imageShape, imgModel);
			arLuciadShapeHashMap.put(imgModel, imageShape);
	}
	public static void updateIcon(KImage model) {
		if(!arLuciadShapeHashMap.containsKey(model)){
			System.err.println("OFF SCREEN MISSING LUCIAD SHAPE"+ model.getId());
			return;
		}
		OrientationLonLatHeightPointModel imageShape =  (OrientationLonLatHeightPointModel) arLuciadShapeHashMap.get(model);
		imageShape.move3D(model.getLocation().getX(), model.getLocation().getY(), model.getLocation().getZ());
		drawingLayer.getModel().elementChanged(imageShape, ILcdModel.FIRE_NOW);
		
	}
	private static ILcdModel loadLocalMap(String mapLocation) throws IOException {
		
		File mapDir = new File(mapLocation);
		String mapPath = mapDir.getAbsolutePath();
		mapPath = mapPath.replace("\\", "\\\\");
		ILcdModel mapModel = decoder.decode(mapPath);

		return mapModel;
	}
	public static void setBound(Rectangle rect) {
		frame.setBounds(rect);
	}
	

}
