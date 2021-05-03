package gov.dsta.vca;

import java.awt.Color;
import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.HashMap;

public abstract class ItemParser {
	
	private static final int _255 = 255;
	public static final String MISSING_CURLY_BRACE = "missing curly brace";
	public static final String INVALID_NUMBER_STRING = "invalid number (double or integer) string";
	public static final String MISSING_DOUBLE_QUOTE = "missing double quote";
	public static final String EMPTY_ATTRIBUTE = "empty attribute string";
	
	public static final String WHITE_COLOR = "white";
	public static final String BLACK_COLOR = "black";
	public static final String GRAY_COLOR = "gray";
	public static final String RED_COLOR = "red";
	public static final String GREEN_COLOR = "green";
	public static final String BLUE_COLOR = "blue";
	public static final String YELLOW_COLOR = "yellow";
	public static final String MAGENTA_COLOR = "magenta";
	public static final String CYAN_COLOR = "cyan";
	public static final String ORANGE_COLOR = "orange";

	//public static final int REDIX = 10;
    public static final int PLUS = 43;
    public static final int MINUS = 45;
    public static final int DECIMAL_DOT = 46;
    public static final int DIGIT_0 = 48;
    public static final int DIGIT_3 = 51;
    public static final int DIGIT_7 = 55;
    public static final int DIGIT_9 = 57;
    public static final int COLON = 58;    
    public static final int DOUBLE_QUOTE = 34;
    public static final int SINGLE_QUOTE = 39;
    public static final int CURLY_BRACE_OPEN = 123;
    public static final int CURLY_BRACE_CLOSE = 125;
    public static final int BRACKET_OPEN = 91;
    public static final int BRACKET_CLOSE = 93;
    public static final int SPACE = 32;
    public static final int CR = 13;
    public static final int FF = 12;
    public static final int LF = 10;
    public static final int BEL = 7;
    public static final int BS = 8;
    public static final int TAB = 9;
    public static final int VT = 11;
    public static final int END_OF_STREAM = -1;

    public static final int UPPERCASE_E = 69;
    public static final int UPPERCASE_P = 80;
    public static final int SLASH = 92;
    public static final int LOWERCASE_A = 97;
    public static final int LOWERCASE_B = 98;
    public static final int LOWERCASE_E = 101;
    public static final int LOWERCASE_F = 102;
    public static final int LOWERCASE_N = 110;
    public static final int LOWERCASE_R = 114;
    public static final int LOWERCASE_T = 116;
    public static final int LOWERCASE_V = 118;
    
    // internal buffer
    private static final int BUFFER_SIZE = 100;
	private InputStream _inputStream = null;
    private BufferedReader _bufferedReader = null;
    private char[] _charBuffer = null;
    private boolean _nextByteReady = false;
    private int _nextByte = 0;

    protected int _lineNumber = 1;
	
	private static final String MSG_PREFIX = "@ItemParser:";
	
	public synchronized HashMap<Integer, IItem> load(String filename) throws IOException {
		_inputStream = new FileInputStream(filename);
		_bufferedReader = new BufferedReader(new InputStreamReader(_inputStream));

		HashMap<Integer, IItem> map = new HashMap<>();
		
		_charBuffer = new char[BUFFER_SIZE];
		_nextByteReady = false;
		
		for (; peek() != END_OF_STREAM; skipWhiteSpace()) {
			IItem item  = parseItem();
			if (item != null)
				map.put(item.getId(), item);
		}
		
		return map;
	}
	
	// subclass needs to implement
	public abstract IItem parseItem() throws IOException;
	
	public void close()	{
		try {
			if (_inputStream != null)
				_inputStream.close();
		
			if (_bufferedReader != null)
				_bufferedReader.close();
		}
		catch(IOException e){

		}

		_inputStream = null;
		_bufferedReader = null;
	}
	
    // trim the colon in the end of string
    protected String trimColon(String str0) {
    	if (str0 == null)
    		return "";
    	
    	String str = str0.trim();
    	if (str.isEmpty())
    		return "";
    	
    	if (Character.getNumericValue(str.charAt(str.length() - 1)) == COLON)
    		str = str.substring(0,  str.length() - 1);
    	
    	return str;
    }
    
    // Read next non-space character. Skip white space before reading next
    protected final int readNext() throws IOException
    {
		skipWhiteSpace();
		return read();
    }
	
    /**
     * Return the value of next character.
     */
    protected final int peek() throws IOException {
        if (_nextByteReady) {
            return _nextByte;
        } else {
            _nextByte = read();
            _nextByteReady = true;

            return _nextByte;
        }
    }
    
    /**
     * Read and return next character. In particular, it will return LF if a CR followed by LF is found.
     */
    // read a single character (0 - 65535)
    // CR + LF --> return LF;
    // CR --> return CR;
    // LF --> return LF;
    // If others, simply return it
    protected final int read() throws IOException {
        if (_nextByteReady) {
            _nextByteReady = false;
            return _nextByte;
        }

        int next  = _bufferedReader.read();
        
        if (next == CR) {
        	
        	_lineNumber++;

            int nextToCR = _bufferedReader.read();
            if (nextToCR == LF) {
            	next = nextToCR;
            } else {
            	_nextByteReady = true;
                _nextByte = nextToCR;
            }
        } else if (next == LF) {
        	_lineNumber++;
        }

        return next;
    }
    
    /**
     * Read till a non-whitespace char is found or end-of-file is encountered.
     * SPACE, LF, CR and TAB are regarded as whitespace characters.
     */
    protected final void skipWhiteSpace() throws IOException {
    	
        int next;

        for (; ((next = peek()) != END_OF_STREAM) && isWhiteSpace(next); read()) {
            ;
        }
    }
    
    /**
     * Return true if the char value is SPACE, LF, CR or TAB.
     * @param next
     * @return
     */
    protected boolean isWhiteSpace(int next) {
    	if (next == SPACE || next == LF || next == CR || next == TAB)
    		return true;
    	
    	return false;
    }
    
    /**
     * Return the string sitting between two whitespace characters.
     */
    protected final String readUnquotedString() throws IOException {
	    skipWhiteSpace();
	
	    int count = 0;
	    int next;
	
	    while (((next = peek()) != END_OF_STREAM) && (!isWhiteSpace(next))) {
	        
	        if (count >= _charBuffer.length)
	        	extendBuffer();
	
//	        _charBuffer[count++] = (char) read();
	        _charBuffer[count++] = Character.toChars(read())[0];
	//        _charBuffer[count++] = Character.forDigit(read(), REDIX);
	    }
	    
    	return String.copyValueOf(_charBuffer, 0, count);
    }
    
    /**
     * If invoked, the function extends the working buffer by another BUFFER_SIZE characters on top
     * of the existing length.
     */
    protected void extendBuffer()
    {
        char[] extended = new char[_charBuffer.length + BUFFER_SIZE];
        System.arraycopy(_charBuffer, 0, extended, 0, _charBuffer.length);
        _charBuffer = extended;
    }
    
    protected static String getMessage(String message, int lineNumber) {
		return "[line:" + lineNumber + "] " + message;
	}
	
    protected int getLineNumber() {
		return _lineNumber;
	}

    // String with double quote " ... " or '...'
    protected final String readString() throws IOException {
        skipWhiteSpace();
        
        int charValue = read();

        if (charValue != DOUBLE_QUOTE && charValue != SINGLE_QUOTE)
            throw new IOException(getMessage(MISSING_DOUBLE_QUOTE, _lineNumber));
        
        charValue = read();

        int count = 0;

        // read until next quote(") or (') or end of file
        while ((charValue != DOUBLE_QUOTE) && (charValue != SINGLE_QUOTE) && (charValue != END_OF_STREAM)) {
            
        	int next;

            if (charValue == SLASH) {
            	
            	next = read();

                int l = next;

                // special character ?
                if ((next >= DIGIT_0) && (next <= DIGIT_7)) {
                	next -= DIGIT_0;

                    int i1 = read();

                    if ((i1 >= DIGIT_0) && (i1 <= DIGIT_7)) {
                        next = (next << 3) + (i1 - DIGIT_0);
                        i1 = read();

                        if ((i1 >= DIGIT_0) && (i1 <= DIGIT_7) && (l <= DIGIT_3)) {
                            next = (next << 3) + (i1 - DIGIT_0);
                            charValue = read();
                        } else {
                        	charValue = i1;
                        }
                    } else {
                    	charValue = i1;
                    }
                } else {
                    switch (next) {
                    case LOWERCASE_A: // 'a'
                    	next = BEL;
                        break;
                    case LOWERCASE_B: // 'b'
                    	next = BS;
                        break;
                    case LOWERCASE_F: // 'f'
                    	next = FF;
                        break;
                    case LOWERCASE_N: // 'n'
                    	next = LF;
                        break;
                    case LOWERCASE_R: // 'r'
                    	next = CR;
                        break;
                    case LOWERCASE_T: // 't'
                    	next = TAB;
                        break;
                    case LOWERCASE_V: // 'v'
                    	next = VT;
                        break;
                        default :
                        break;
                    }

                    charValue = read();
                }
            } else {
            	next = charValue;
                charValue = read();
            }

            if (count >= _charBuffer.length)
            	extendBuffer();

//            _charBuffer[count++] = (char) next;
            _charBuffer[count++] = Character.toChars(next)[0];
            
         //   _charBuffer[count++] = Character.forDigit(next, 10);
        }

        return String.copyValueOf(_charBuffer, 0, count);
    }
    
    /**
     * Read an integer string (-, 0-9), and convert the string into an integer and return. 
     */
    protected final int readInt() throws IOException {
        skipWhiteSpace();

        int charValue = peek();
        int count = 0;

        if ((charValue != MINUS) && (charValue < DIGIT_0 || charValue > DIGIT_9)) {
            throw new IOException(getMessage(INVALID_NUMBER_STRING, _lineNumber));
        }

        do {
            if (count >= _charBuffer.length)
            	extendBuffer();

//            _charBuffer[count++] = (char) read();
            _charBuffer[count++] = Character.toChars(read())[0];
      //      _charBuffer[count++] = Character.forDigit(read(), REDIX);
            
            
        } while (((charValue = peek()) >= DIGIT_0) && (charValue <= DIGIT_9));

        return Integer.valueOf(String.copyValueOf(_charBuffer, 0, count));
    }
    
    /**
     * Read a string: xxx: and return xxx; or '}' encountered and return empty string 
     */
    protected final String readAttribute() throws IOException {
        skipWhiteSpace();

        int charValue = peek();
        if (charValue == CURLY_BRACE_CLOSE)
        	return "";	// no more attribute
        
        int count = 0;

        if (charValue == COLON)
            throw new IOException(getMessage(EMPTY_ATTRIBUTE, _lineNumber));

        do {
            if (count >= _charBuffer.length)
            	extendBuffer();

//            _charBuffer[count++] = (char) read();
            _charBuffer[count++] = Character.toChars(read())[0];
       //     _charBuffer[count++] = Character.forDigit(read(), REDIX);
            
        } while ((charValue = peek()) != COLON);
        
        charValue = read();	// read colon;
        
        return String.copyValueOf(_charBuffer, 0, count);
    }

    
	/**
	 * Two formats: either value "255 18 232", or name "cyan", "white", where 255 is the maximum number
	 */
	public static Color toColor(String colorStr, Color defaultColor)
	{
		if (colorStr == null || colorStr.trim().isEmpty())
			return defaultColor;
		
		String[] splited = colorStr.trim().split("\\s+");
		if (splited == null)
			return defaultColor;
		
		Color color = null;
		
		if (splited.length < 3) {
			color = getColor(splited[0]);
		}
		else {
			color = new Color(Integer.valueOf(splited[0]), Integer.valueOf(splited[1]), Integer.valueOf(splited[2]));
		}
		
		if (color == null)
			color = defaultColor;
		
		return color;
	}
	
	public static Color getColor(String colorName) {
		if (colorName == null || colorName.trim().isEmpty())
			return null;
		
		Color color = null;
		
		int alpha = _255;
		String name = colorName.trim().toLowerCase();
		if (WHITE_COLOR.equals(name))
			color = new Color(255, 255, 255, alpha);
		else if (BLACK_COLOR.equals(name))
			color = new Color(0, 0, 0, alpha);
		else if (GRAY_COLOR.equals(name))
			color = new Color(192, 192, 192, alpha);
		else if (RED_COLOR.equals(name))
			color = new Color(255, 0, 0, alpha);
		else if (GREEN_COLOR.equals(name))
			color = new Color(0, 255, 0, alpha);
		else if (BLUE_COLOR.equals(name))
			color = new Color(0, 0, 255, alpha);
		else if (YELLOW_COLOR.equals(name))
			color = new Color(255, 255, 0, alpha);
		else if (MAGENTA_COLOR.equals(name))
			color = new Color(255, 0, 255, alpha);
		else if (CYAN_COLOR.equals(name))
			color = new Color(0, 255, 255, alpha);
		else if (ORANGE_COLOR.equals(name))
			color = new Color(255, 165, 0, alpha);
		
		return color;
	}

}
