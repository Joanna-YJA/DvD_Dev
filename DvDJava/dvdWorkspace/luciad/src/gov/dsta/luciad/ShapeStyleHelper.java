package gov.dsta.luciad;

import java.awt.Color;
import java.awt.Dimension;
import java.awt.Image;
import java.awt.Point;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Set;

import javax.imageio.ImageIO;

import com.luciad.gui.TLcdHaloIcon;
import com.luciad.gui.TLcdImageIcon;
import com.luciad.shape.ILcdShape;
import com.luciad.shape.shape2D.TLcdLonLatPoint;
import com.luciad.view.lightspeed.layer.ILspInteractivePaintableLayer;
import com.luciad.view.lightspeed.layer.ILspLayer;
import com.luciad.view.lightspeed.layer.TLspPaintRepresentationState;
import com.luciad.view.lightspeed.style.ILspTexturedStyle.TextureCoordinatesMode;
import com.luciad.view.lightspeed.style.ILspWorldElevationStyle.ElevationMode;
import com.luciad.view.lightspeed.style.TLspFillStyle;
import com.luciad.view.lightspeed.style.TLspFillStyle.Builder;
import com.luciad.view.lightspeed.style.TLspFillStyle.StipplePattern;
import com.luciad.view.lightspeed.style.TLspIconStyle;
import com.luciad.view.lightspeed.style.TLspIconStyle.ScalingMode;
import com.luciad.view.lightspeed.style.TLspLineStyle;
import com.luciad.view.lightspeed.style.TLspLineStyle.DashPattern;
import com.luciad.view.lightspeed.style.TLspVerticalLineStyle;
import com.luciad.view.lightspeed.style.TLspViewDisplacementStyle;
import com.luciad.view.lightspeed.style.styler.TLspEditableStyler;

import gov.dsta.luciad.interfaces.IKGraphic;
import gov.dsta.luciad.interfaces.IKLayer;
import gov.dsta.luciad.interfaces.IKPoint;
import gov.dsta.luciad.interfaces.KImage;
import gov.dsta.luciad.interfaces.KPoint;

public class ShapeStyleHelper {

	private static final String MSG_PREFIX = "@GIS:";
	
	private static final float STYLER_OPACITY = 0.2f;

	public static void applyStyleToPointShape(IKGraphic aModel, ILcdShape shape, ILspLayer aLayer, IKLayer kLayer) {

		Boolean isReducedDisplaySupported = kLayer.isReducedDisplaySupported();

		ILspInteractivePaintableLayer lspLayer = (ILspInteractivePaintableLayer) aLayer;
		TLspEditableStyler mainStyler = (TLspEditableStyler) lspLayer
				.getStyler(TLspPaintRepresentationState.REGULAR_BODY);
		TLspEditableStyler selStyler = (TLspEditableStyler) lspLayer
				.getStyler(TLspPaintRepresentationState.SELECTED_BODY);
		
		List<com.luciad.view.lightspeed.style.ALspStyle> mainStyleList = new ArrayList<com.luciad.view.lightspeed.style.ALspStyle>();
		List<com.luciad.view.lightspeed.style.ALspStyle> selStyleList = new ArrayList<com.luciad.view.lightspeed.style.ALspStyle>();

		//Get the image
		Image image = null;
		 if(aModel instanceof KImage) {
			
			KImage imgModel = (KImage) aModel;
			TLspIconStyle iconStyle = TLspIconStyle
					.newBuilder()
					.icon(new TLcdHaloIcon(new TLcdImageIcon(imgModel.getImage()), Color.WHITE, 2))
					// Set icons to have fixed view coordinates
					//.scalingMode(ScalingMode.VIEW_SCALING)
					.scalingMode(ScalingMode.WORLD_SCALING_CLAMPED)
					.useOrientation(true)
					.zOrder(1).scale(1.0)
					.elevationMode(ElevationMode.OBJECT_DEPENDENT)
					// Set the icons' alpha value
					.opacity(1.0f)
					.useOrientation(false)
					.build();
			double aOrientation = imgModel.getOrientation();
			
			if (shape instanceof OrientedLonLatPointModel) {
				((OrientedLonLatPointModel) shape).setOrientation(aOrientation);

			}
			// Set selected image if there is any.
			Image selectedImage = imgModel.getSelectedImg();

			// If no selected image.
			// Use default image.
			if (selectedImage == null) {
				selectedImage = imgModel.getImage();
			}

			// Style selected image.
			TLspIconStyle selIconStyle = TLspIconStyle.newBuilder().useOrientation(true)
					.icon(new TLcdHaloIcon(new TLcdImageIcon(selectedImage), Color.WHITE, 2)).zOrder(0).build();

			// reduced style
			if (isReducedDisplaySupported) {

				TLspIconStyle reducedIconStyle = TLspIconStyle
						.newBuilder()
						.icon(new TLcdHaloIcon(new TLcdImageIcon(imgModel.getReduceDisplayImg()), Color.WHITE, 2))
						// Set icons to have fixed view coordinates
						.scalingMode(ScalingMode.VIEW_SCALING)
						.useOrientation(true).zOrder(1).scale(1.0)
						.elevationMode(ElevationMode.OBJECT_DEPENDENT)
						// Set the icons' alpha value
						.opacity(1.0f)
						.useOrientation(false)
						.build();
			
				((ReducedDisplayImageStyler) mainStyler).setReducedStyle(lspLayer.getModel(), shape,
						Arrays.<com.luciad.view.lightspeed.style.ALspStyle> asList(reducedIconStyle));

				((ReducedDisplayImageStyler) selStyler).setReducedStyle(lspLayer.getModel(),
						shape, Arrays.<com.luciad.view.lightspeed.style.ALspStyle> asList(selIconStyle,
								reducedIconStyle));
			}
			TLspVerticalLineStyle verticalLine = TLspVerticalLineStyle.newBuilder().color(Color.BLACK).build();
			mainStyleList.add(verticalLine);
			selStyleList.add(verticalLine);

			mainStyleList.add(iconStyle);
			selStyleList.add(selIconStyle);
			selStyleList.add(iconStyle);
			
		}
		
		mainStyler.setStyle(lspLayer.getModel(), shape, mainStyleList);
		selStyler.setStyle(lspLayer.getModel(), shape, selStyleList);

	}
	
	
	
	private static Image createCrossMarkerIcon() throws IOException {
		//TODO read from config?
		File dir1 = new File(System.getProperty("user.dir") + "/icons/type/enemy/dummy.png");	
		BufferedImage image = ImageIO.read(dir1);
		return image;
	}
	
	private static TLspIconStyle createIconStyle(IKPoint offSetPoint, Image img, int zOrder ) {
		// Define icon style
		TLspIconStyle iconStyle = TLspIconStyle.newBuilder()
				.offset(offSetPoint.getX(),offSetPoint.getY())
				.icon(new TLcdHaloIcon(new TLcdImageIcon(img), Color.WHITE, 2))
				// Set icons to have fixed view coordinates
				.scalingMode(ScalingMode.VIEW_SCALING).useOrientation(true)
				.zOrder(zOrder).scale(1.0)
				.elevationMode(ElevationMode.OBJECT_DEPENDENT)
				// Set the icons' alpha value
				.opacity(1.0f)
				.useOrientation(false)
				.build();
		
		return iconStyle;
	}
	
	

	/*
	 * For internal use when there isn't any use of KModels
	 */
	public static void applyStyleToShape(ILcdShape shape, ILspLayer aLayer, Color fillColor, Color lineColor,
			boolean isFilled) {

		ILspInteractivePaintableLayer lspLayer = (ILspInteractivePaintableLayer) aLayer;
		TLspEditableStyler mainStyler = (TLspEditableStyler) lspLayer
				.getStyler(TLspPaintRepresentationState.REGULAR_BODY);
		TLspEditableStyler selStyler = (TLspEditableStyler) lspLayer
				.getStyler(TLspPaintRepresentationState.SELECTED_BODY);
		List<com.luciad.view.lightspeed.style.ALspStyle> mainStyleList = new ArrayList<com.luciad.view.lightspeed.style.ALspStyle>();
		List<com.luciad.view.lightspeed.style.ALspStyle> selStyleList = new ArrayList<com.luciad.view.lightspeed.style.ALspStyle>();

		String lineStyleVal = "SOLID";
		boolean isOutlined = true;
		if (isOutlined) {
			DashPattern dashPattern;
			int SOLID_SCALE = 0;
			int DASH_SCALE = 10;

			//if 0 means solid
			if ("SOLID".equalsIgnoreCase(lineStyleVal)) {
				dashPattern = new TLspLineStyle.DashPattern(DashPattern.SOLID, SOLID_SCALE);
			}
			// if not 0 means dash
			else {
				dashPattern = new TLspLineStyle.DashPattern(DashPattern.LONG_DASH, DASH_SCALE);
			}

			com.luciad.view.lightspeed.style.ALspStyle lineStyle = TLspLineStyle.newBuilder()
					.color(lineColor)
					//.dashPattern(dashPattern)
					.dashPattern(dashPattern).width(2)
					.elevationMode(ElevationMode.ON_TERRAIN)
					//.opacity(aOpacity)
					.build();

			com.luciad.view.lightspeed.style.ALspStyle selLineStyle = TLspLineStyle.newBuilder()
					.color(lineColor)
					//.dashPattern(dashPattern)
					.width(2).dashPattern(dashPattern)
					.elevationMode(ElevationMode.ON_TERRAIN)
					//.opacity(aOpacity)
					.build();

			mainStyleList.add(lineStyle);
			selStyleList.add(selLineStyle);

		}
		
		if (isFilled) {
			Builder<?> fillStylebuilder = TLspFillStyle.newBuilder().elevationMode(ElevationMode.ON_TERRAIN)
					.color(fillColor);

			mainStyleList.add(fillStylebuilder.build());
			selStyleList.add(fillStylebuilder.build());

		}

		mainStyler.setStyle(lspLayer.getModel(), shape, mainStyleList);
		selStyler.setStyle(lspLayer.getModel(), shape, selStyleList);
	}


}
