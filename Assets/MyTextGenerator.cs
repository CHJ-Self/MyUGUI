using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine.Scripting;
using static UnityEngine.UI.RequiredByNativeCodeAttribute;

namespace UnityEngine.UI
{
	/// <summary>
	///   <para>Class that can be used to generate text for rendering.</para>
	/// </summary>
	[UsedByNativeCode]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class MyTextGenerator : IDisposable
	{
		internal IntPtr m_Ptr;
		private string m_LastString;
		private MyTextGenerationSettings m_LastSettings;
		private bool m_HasGenerated;
		private TextGenerationError m_LastValid;
		private readonly List<UIVertex> m_Verts;
		private readonly List<UICharInfo> m_Characters;
		private readonly List<UILineInfo> m_Lines;
		private bool m_CachedVerts;
		private bool m_CachedCharacters;
		private bool m_CachedLines;
		private static int s_NextId = 0;
		private int m_Id;
		private static readonly Dictionary<int, WeakReference> s_Instances = new Dictionary<int, WeakReference>();
		/// <summary>
		///   <para>Array of generated vertices.</para>
		/// </summary>
		public IList<UIVertex> verts
		{
			get
			{
				if (!this.m_CachedVerts)
				{
					this.GetVertices(this.m_Verts);
					this.m_CachedVerts = true;
				}
				return this.m_Verts;
			}
		}
		/// <summary>
		///   <para>Array of generated characters.</para>
		/// </summary>
		public IList<UICharInfo> characters
		{
			get
			{
				if (!this.m_CachedCharacters)
				{
					this.GetCharacters(this.m_Characters);
					this.m_CachedCharacters = true;
				}
				return this.m_Characters;
			}
		}
		/// <summary>
		///   <para>Information about each generated text line.</para>
		/// </summary>
		public IList<UILineInfo> lines
		{
			get
			{
				if (!this.m_CachedLines)
				{
					this.GetLines(this.m_Lines);
					this.m_CachedLines = true;
				}
				return this.m_Lines;
			}
		}
		/// <summary>
		///   <para>Extents of the generated text in rect format.</para>
		/// </summary>
		public Rect rectExtents
		{
			get
			{
				Rect result;
				this.INTERNAL_get_rectExtents(out result);
				return result;
			}
		}
		/// <summary>
		///   <para>Number of vertices generated.</para>
		/// </summary>
		public extern int vertexCount
		{
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		/// <summary>
		///   <para>The number of characters that have been generated.</para>
		/// </summary>
		public extern int characterCount
		{
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		/// <summary>
		///   <para>The number of characters that have been generated and are included in the visible lines.</para>
		/// </summary>
		public int characterCountVisible
		{
			get
			{
				return this.characterCount - 1;
			}
		}
		/// <summary>
		///   <para>Number of text lines generated.</para>
		/// </summary>
		public extern int lineCount
		{
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		/// <summary>
		///   <para>The size of the font that was found if using best fit mode.</para>
		/// </summary>
		public extern int fontSizeUsedForBestFit
		{
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		/// <summary>
		///   <para>Create a TextGenerator.</para>
		/// </summary>
		/// <param name="initialCapacity"></param>
		public MyTextGenerator() : this(50)
		{
		}
		/// <summary>
		///   <para>Create a TextGenerator.</para>
		/// </summary>
		/// <param name="initialCapacity"></param>
		public MyTextGenerator(int initialCapacity)
		{
			this.m_Verts = new List<UIVertex>((initialCapacity + 1) * 4);
			this.m_Characters = new List<UICharInfo>(initialCapacity + 1);
			this.m_Lines = new List<UILineInfo>(20);
			this.Init();
			object obj = MyTextGenerator.s_Instances;
			Monitor.Enter(obj);
			try
			{
				this.m_Id = MyTextGenerator.s_NextId++;
				MyTextGenerator.s_Instances.Add(this.m_Id, new WeakReference(this));
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}
		~MyTextGenerator()
		{
			((IDisposable)this).Dispose();
		}
		void IDisposable.Dispose()
		{
			object obj = MyTextGenerator.s_Instances;
			Monitor.Enter(obj);
			try
			{
				MyTextGenerator.s_Instances.Remove(this.m_Id);
			}
			finally
			{
				Monitor.Exit(obj);
			}
			this.Dispose_cpp();
		}
		[RequiredByNativeCode]
		internal static void InvalidateAll()
		{
			object obj = MyTextGenerator.s_Instances;
			Monitor.Enter(obj);
			try
			{
				foreach (KeyValuePair<int, WeakReference> current in MyTextGenerator.s_Instances)
				{
					WeakReference value = current.Value;
					if (value.IsAlive)
					{
						(value.Target as TextGenerator).Invalidate();
					}
				}
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}
		private MyTextGenerationSettings ValidatedSettings(MyTextGenerationSettings settings)
		{
			MyTextGenerationSettings result;
			if (settings.font != null && settings.font.dynamic)
			{
				result = settings;
			}
			else
			{
				if (settings.fontSize != 0 || settings.fontStyle != FontStyle.Normal)
				{
					if (settings.font != null)
					{
						Debug.LogWarningFormat(settings.font, "Font size and style overrides are only supported for dynamic fonts. Font '{0}' is not dynamic.", new object[]
						{
							settings.font.name
						});
					}
					settings.fontSize = 0;
					settings.fontStyle = FontStyle.Normal;
				}
				if (settings.resizeTextForBestFit)
				{
					if (settings.font != null)
					{
						Debug.LogWarningFormat(settings.font, "BestFit is only supported for dynamic fonts. Font '{0}' is not dynamic.", new object[]
						{
							settings.font.name
						});
					}
					settings.resizeTextForBestFit = false;
				}
				result = settings;
			}
			return result;
		}
		/// <summary>
		///   <para>Mark the text generator as invalid. This will force a full text generation the next time Populate is called.</para>
		/// </summary>
		public void Invalidate()
		{
			this.m_HasGenerated = false;
		}
		public void GetCharacters(List<UICharInfo> characters)
		{
			this.GetCharactersInternal(characters);
		}
		public void GetLines(List<UILineInfo> lines)
		{
			this.GetLinesInternal(lines);
		}
		public void GetVertices(List<UIVertex> vertices)
		{
			this.GetVerticesInternal(vertices);
		}
		/// <summary>
		///   <para>Given a string and settings, returns the preferred width for a container that would hold this text.</para>
		/// </summary>
		/// <param name="str">Generation text.</param>
		/// <param name="settings">Settings for generation.</param>
		/// <returns>
		///   <para>Preferred width.</para>
		/// </returns>
		public float GetPreferredWidth(string str, MyTextGenerationSettings settings)
		{
			settings.horizontalOverflow = HorizontalWrapMode.Overflow;
			settings.verticalOverflow = VerticalWrapMode.Overflow;
			settings.updateBounds = true;
			this.Populate(str, settings);
			return this.rectExtents.width;
		}
		/// <summary>
		///   <para>Given a string and settings, returns the preferred height for a container that would hold this text.</para>
		/// </summary>
		/// <param name="str">Generation text.</param>
		/// <param name="settings">Settings for generation.</param>
		/// <returns>
		///   <para>Preferred height.</para>
		/// </returns>
		public float GetPreferredHeight(string str, MyTextGenerationSettings settings)
		{
			settings.verticalOverflow = VerticalWrapMode.Overflow;
			settings.updateBounds = true;
			this.Populate(str, settings);
			return this.rectExtents.height;
		}
		/// <summary>
		///   <para>Will generate the vertices and other data for the given string with the given settings.</para>
		/// </summary>
		/// <param name="str">String to generate.</param>
		/// <param name="settings">Generation settings.</param>
		/// <param name="context">The object used as context of the error log message, if necessary.</param>
		/// <returns>
		///   <para>True if the generation is a success, false otherwise.</para>
		/// </returns>
		public bool PopulateWithErrors(string str, MyTextGenerationSettings settings, GameObject context)
		{
			TextGenerationError textGenerationError = this.PopulateWithError(str, settings);
			bool result;
			if (textGenerationError == TextGenerationError.None)
			{
				result = true;
			}
			else
			{
				if ((textGenerationError & TextGenerationError.CustomSizeOnNonDynamicFont) != TextGenerationError.None)
				{
					Debug.LogErrorFormat(context, "Font '{0}' is not dynamic, which is required to override its size", new object[]
					{
						settings.font
					});
				}
				if ((textGenerationError & TextGenerationError.CustomStyleOnNonDynamicFont) != TextGenerationError.None)
				{
					Debug.LogErrorFormat(context, "Font '{0}' is not dynamic, which is required to override its style", new object[]
					{
						settings.font
					});
				}
				result = false;
			}
			return result;
		}
		/// <summary>
		///   <para>Will generate the vertices and other data for the given string with the given settings.</para>
		/// </summary>
		/// <param name="str">String to generate.</param>
		/// <param name="settings">Settings.</param>
		public bool Populate(string str, MyTextGenerationSettings settings)
		{
			TextGenerationError textGenerationError = this.PopulateWithError(str, settings);
			return textGenerationError == TextGenerationError.None;
		}
		private TextGenerationError PopulateWithError(string str, MyTextGenerationSettings settings)
		{
			TextGenerationError lastValid;
			if (this.m_HasGenerated && str == this.m_LastString && settings.Equals(this.m_LastSettings))
			{
				lastValid = this.m_LastValid;
			}
			else
			{
				this.m_LastValid = this.PopulateAlways(str, settings);
				lastValid = this.m_LastValid;
			}
			return lastValid;
		}
		private TextGenerationError PopulateAlways(string str, MyTextGenerationSettings settings)
		{
			this.m_LastString = str;
			this.m_HasGenerated = true;
			this.m_CachedVerts = false;
			this.m_CachedCharacters = false;
			this.m_CachedLines = false;
			this.m_LastSettings = settings;
			MyTextGenerationSettings textGenerationSettings = this.ValidatedSettings(settings);
			TextGenerationError textGenerationError;
			this.Populate_Internal(str, textGenerationSettings.font, textGenerationSettings.color, textGenerationSettings.fontSize, textGenerationSettings.scaleFactor, textGenerationSettings.lineSpacing, textGenerationSettings.fontStyle, textGenerationSettings.richText, textGenerationSettings.resizeTextForBestFit, textGenerationSettings.resizeTextMinSize, textGenerationSettings.resizeTextMaxSize, textGenerationSettings.verticalOverflow, textGenerationSettings.horizontalOverflow, textGenerationSettings.updateBounds, textGenerationSettings.textAnchor, textGenerationSettings.generationExtents, textGenerationSettings.pivot, textGenerationSettings.generateOutOfBounds, textGenerationSettings.alignByGeometry, out textGenerationError);
			this.m_LastValid = textGenerationError;
			return textGenerationError;
		}
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void Init();
		[GeneratedByOldBindingsGenerator, ThreadAndSerializationSafe]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void Dispose_cpp();
		internal bool Populate_Internal(string str, Font font, Color color, int fontSize, float scaleFactor, float lineSpacing, FontStyle style, bool richText, bool resizeTextForBestFit, int resizeTextMinSize, int resizeTextMaxSize, VerticalWrapMode verticalOverFlow, HorizontalWrapMode horizontalOverflow, bool updateBounds, TextAnchor anchor, Vector2 extents, Vector2 pivot, bool generateOutOfBounds, bool alignByGeometry, out TextGenerationError error)
		{
			uint num = 0u;
			bool result;
			if (font == null)
			{
				error = TextGenerationError.NoFont;
				result = false;
			}
			else
			{
				bool flag = this.Populate_Internal_cpp(str, font, color, fontSize, scaleFactor, lineSpacing, style, richText, resizeTextForBestFit, resizeTextMinSize, resizeTextMaxSize, (int)verticalOverFlow, (int)horizontalOverflow, updateBounds, anchor, extents.x, extents.y, pivot.x, pivot.y, generateOutOfBounds, alignByGeometry, out num);
				error = (TextGenerationError)num;
				result = flag;
			}
			return result;
		}
		internal bool Populate_Internal_cpp(string str, Font font, Color color, int fontSize, float scaleFactor, float lineSpacing, FontStyle style, bool richText, bool resizeTextForBestFit, int resizeTextMinSize, int resizeTextMaxSize, int verticalOverFlow, int horizontalOverflow, bool updateBounds, TextAnchor anchor, float extentsX, float extentsY, float pivotX, float pivotY, bool generateOutOfBounds, bool alignByGeometry, out uint error)
		{
			return MyTextGenerator.INTERNAL_CALL_Populate_Internal_cpp(this, str, font, ref color, fontSize, scaleFactor, lineSpacing, style, richText, resizeTextForBestFit, resizeTextMinSize, resizeTextMaxSize, verticalOverFlow, horizontalOverflow, updateBounds, anchor, extentsX, extentsY, pivotX, pivotY, generateOutOfBounds, alignByGeometry, out error);
		}
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool INTERNAL_CALL_Populate_Internal_cpp(MyTextGenerator self, string str, Font font, ref Color color, int fontSize, float scaleFactor, float lineSpacing, FontStyle style, bool richText, bool resizeTextForBestFit, int resizeTextMinSize, int resizeTextMaxSize, int verticalOverFlow, int horizontalOverflow, bool updateBounds, TextAnchor anchor, float extentsX, float extentsY, float pivotX, float pivotY, bool generateOutOfBounds, bool alignByGeometry, out uint error);
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void INTERNAL_get_rectExtents(out Rect value);
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void GetVerticesInternal(object vertices);
		/// <summary>
		///   <para>Returns the current UILineInfo.</para>
		/// </summary>
		/// <returns>
		///   <para>Vertices.</para>
		/// </returns>
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern UIVertex[] GetVerticesArray();
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void GetCharactersInternal(object characters);
		/// <summary>
		///   <para>Returns the current UICharInfo.</para>
		/// </summary>
		/// <returns>
		///   <para>Character information.</para>
		/// </returns>
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern UICharInfo[] GetCharactersArray();
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void GetLinesInternal(object lines);
		/// <summary>
		///   <para>Returns the current UILineInfo.</para>
		/// </summary>
		/// <returns>
		///   <para>Line information.</para>
		/// </returns>
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern UILineInfo[] GetLinesArray();

		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, Inherited = false)]
		internal class UsedByNativeCodeAttribute : Attribute
		{
			public string Name
			{
				get;
				set;
			}
			public UsedByNativeCodeAttribute()
			{
			}
			public UsedByNativeCodeAttribute(string name)
			{
				this.Name = name;
			}
		}
	}

	[Flags]
	internal enum TextGenerationError
	{
		None = 0,
		CustomSizeOnNonDynamicFont = 1,
		CustomStyleOnNonDynamicFont = 2,
		NoFont = 4
	}

	internal class GeneratedByOldBindingsGeneratorAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, Inherited = false)]
	internal class RequiredByNativeCodeAttribute : Attribute
	{
		public string Name
		{
			get;
			set;
		}
		public bool Optional
		{
			get;
			set;
		}
		public RequiredByNativeCodeAttribute()
		{
		}
		public RequiredByNativeCodeAttribute(string name)
		{
			this.Name = name;
		}
		public RequiredByNativeCodeAttribute(bool optional)
		{
			this.Optional = optional;
		}
		public RequiredByNativeCodeAttribute(string name, bool optional)
		{
			this.Name = name;
			this.Optional = optional;
		}

		internal class ThreadAndSerializationSafeAttribute : Attribute
		{
		}
	}
}
