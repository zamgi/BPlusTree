//#define USE_MIN_MAX_VARIABLES

using SysArray = System.Array;

namespace System.Collections.Generic
{
    /// <summary>
    /// 
    /// </summary>
	internal class SortedList_Impl< T > : IEnumerable< T > //: IDictionary< string, T >, ICollection< KeyValuePair< string, T > >
	{
        private const int MAX_CAPACITY_THRESHOLD = 0x7FFFFFFF /*int.MaxValue*/ - 0x400 * 0x400 /*1MB*/; /* => 2146435071 == 0x7fefffff*/
        private static readonly T[] EMPTY_ARRAY = new T[ 0 ];

        private T[] _Array;
		private int _Size;
        private IComparer< T > _Comparer;
#if USE_MIN_MAX_VARIABLES
        private T _Min;
        private T _Max;
#endif
        protected SortedList_Impl() { }
        public SortedList_Impl( IComparer< T > comparer, int capacity )
		{
            _Comparer = comparer;
            _Array    = (capacity == 0) ? EMPTY_ARRAY : new T[ capacity ];
            _Size     = 0;
		}
        public SortedList_Impl( IComparer< T > comparer, int capacity, T t )
        {
            _Comparer   = comparer;
            _Array      = new T[ capacity ];
            _Array[ 0 ] = t;
            _Size       = 1;
#if USE_MIN_MAX_VARIABLES
            _Min = t;
            _Max = t;
#endif
        }

        /*public SortedList< T > SplitInTwo()
        {
            var half = (_Size >> 1);
            var other = new SortedList< T >( _Comparer )
            {
                _Array = new T[ _Array.Length ],
                _Size  = half,
            };
            _Size -= half;
            SysArray.Copy( _Array, _Size, other._Array, 0, half );
            SysArray.Clear( _Array, _Size, half );

            return (other);
        }*/
        public void SplitInTwo< X >( X other ) where X : SortedList_Impl< T >
        {
            var half = (_Size >> 1);

            other._Comparer = _Comparer;
            other._Array    = new T[ _Array.Length ];
            other._Size     = half;

            _Size -= half;
            SysArray.Copy( _Array, _Size, other._Array, 0, half );
            SysArray.Clear( _Array, _Size, half );
#if USE_MIN_MAX_VARIABLES
            _Min = _Array[ 0 ];
            _Max = _Array[ _Size - 1 ];
            other._Min = other._Array[ 0 ];
            other._Max = other._Array[ other._Size - 1 ];
#endif
            //return (other);
        }

        public int CompareWith4_SortedBlockList( T value )
        {
            var d = _Comparer.Compare( this.Min, value );
            if ( 0 <= d ) //value < Min
                return (d);
            //d < 0 
            return (_Comparer.Compare( this.Max, value ));
        }

        public int CompareWith4_SortedBlockSet( T value )
        {
            var d = _Comparer.Compare( this.Min, value );
            if ( 0 <= d ) //value < Min
                return (d);

            d = _Comparer.Compare( this.Max, value );
            return (0 <= d ? 0 : d); //if {value <= Max} => {0 <= d} => 0, else => d;
        }

        public int CompareOtherWith4_BPlusTreeBlock( SortedList_Impl< T > other )
        {
            //must never been empty
            return (_Comparer.Compare( this.Max, other.Min ));

            #region comm. prev.
            /*
            if ( this._Size != 0 )
            {
                if ( other._Size != 0 )
                {
                    return (_Comparer.Compare( this.Max, other.Min ));
                }
                return (1);
            }
            if ( other._Size != 0 )
            {
                return (-1);
            }
            return (0);
            */ 
            #endregion
        }

        public int Capacity
		{
			get => _Array.Length;
			private set
			{
                if ( value != _Array.Length )
				{
                    //if (value < _Size) throw (new ArgumentOutOfRangeException( nameof(value) ));
                    if ( 0 < value )
					{
                        var destinationArray = new T[ value ];
                        if ( 0 < _Size )
						{
                            SysArray.Copy( _Array, 0, destinationArray, 0, _Size );
						}
                        _Array = destinationArray;
					}
                    else
                    {
                        _Array = EMPTY_ARRAY;
                    }
				}
			}
		}
        public int Count => _Size;
        public bool IsFull => (_Array.Length == _Size); 

        public T this[ int index ] => _Array[ index ];
        public T Min
        {
            get
            {
#if DEBUG
                if ( _Size == 0 ) throw (new InvalidOperationException());
#endif                
#if USE_MIN_MAX_VARIABLES
                return (_Min);
#else
                return (_Array[ 0 ]);
#endif
            }
        }
        public T Max
        {
            get
            {
#if DEBUG
                if ( _Size == 0 ) throw (new InvalidOperationException());
#endif
#if USE_MIN_MAX_VARIABLES
                return (_Max);
#else
                return (_Array[ _Size - 1 ]);
#endif
            }
        }
	
        public void Add( T value )
		{
            var index = InternalBinarySearch( value ); 
            if ( 0 <= index )
			{
                throw (new ArgumentException( index.ToString(), nameof(index) )); //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
            }
            Insert( ~index, value );
		}
        public bool TryAdd( T value )
		{
            var index = InternalBinarySearch( value );
            if ( 0 <= index )
			{
                return (false);
			}
            Insert( ~index, value );
            return (true);
		}
        public bool TryAddOrGetExistsValue( T value, out T existsValue )
        {
            var index = InternalBinarySearch( value );
            if ( 0 <= index )
            {
                existsValue = _Array[ index ];
                return (false);
            }
            Insert( ~index, value );
            existsValue = default;
            return (true);
        }
        public void Clear()
		{
            SysArray.Clear( _Array, 0, _Size );
			_Size = 0;
#if USE_MIN_MAX_VARIABLES
            _Min = default;
            _Max = default;
#endif
        }
        public bool Contains( T value ) => (0 <= IndexOfKey( value ));

        public int IndexOfKey( T value )
		{
            var index = InternalBinarySearch( value );
            if ( index < 0 )
            {
                return (-1);
            }
			return (index);
		}
        public int IndexOfKeyCore( T value ) => InternalBinarySearch( value );
        private void Insert( int index, T value )
        {
            if ( _Size == _Array.Length )
            {
                EnsureCapacity( _Size + 1 );
            }
            if ( index < _Size )
            {
                SysArray.Copy( _Array, index, _Array, index + 1, _Size - index );
            }
            _Array[ index ] = value;
#if USE_MIN_MAX_VARIABLES
            _Min = _Array[ 0 ];
            _Max = _Array[ _Size ];
#endif
            _Size++;
        }

		public bool TryGetValue( T value, out T existsValue )
		{
            var index = IndexOfKey( value );
            if ( 0 <= index )
			{
                existsValue = _Array[ index ];
				return (true);
			}
            existsValue = default;
			return (false);
		}
        public T GetValue( T value, T defaultValue = default )
        {
            var index = IndexOfKey( value );
            if ( 0 <= index )
            {
                return (_Array[ index ]);
            }
            return (defaultValue);
        }
        public void RemoveAt( int index )
		{
#if DEBUG
            if ( index < 0 || _Size <= index ) throw (new ArgumentOutOfRangeException( nameof(index) ));
#endif
			_Size--;
            if ( index < _Size )
			{
                SysArray.Copy( _Array, index + 1, _Array, index, _Size - index );
			}
			_Array[ _Size ] = default;
#if USE_MIN_MAX_VARIABLES
            _Min = _Array[ 0 ];
            _Max = _Array[ _Size - 1 ];
#endif
        }
		public bool Remove( T value )
		{
            var index = IndexOfKey( value );
            if ( 0 <= index )
            {
                RemoveAt( index );
            }
            return (0 <= index);
		}

		public void TrimExcess()
		{
            var size = (int) (_Array.Length * 0.9);
            if ( _Size < size )
            {
                Capacity = _Size;
            }
		}
        public void Trim() => Capacity = _Size;

        private void EnsureCapacity( int min )
		{
            #region comm.
            /*int capacity;
            switch ( _Array.Length )
            {
                case 0:  capacity = 1; break;
                case 1:  capacity = 16; break;
                default: capacity = _Array.Length * 2; break;
            }
            */
            /*int capacity = (_Array.Length == 0) ? DEFAULT_CAPACITY : (_Array.Length * 2);*/
            #endregion

            var capacity = (_Array.Length << 1);
            if ( MAX_CAPACITY_THRESHOLD < capacity )
			{
                capacity = MAX_CAPACITY_THRESHOLD;
			}
            if ( capacity < min )
            {
                capacity = min;
            }
			Capacity = capacity;
		}

        private int InternalBinarySearch( T value )
        {
            var i = 0;
            for ( var endIndex = _Size - 1; i <= endIndex; )
            {
                var middleIndex = i + ((endIndex - i) >> 1);
                var d = _Comparer.Compare( _Array[ middleIndex ], value );
                if ( d == 0 )
                {
                    return (middleIndex);
                }

                if ( d < 0 )
                {
                    i = middleIndex + 1;
                }
                else
                {
                    endIndex = middleIndex - 1;
                }
            }
            return (~i);
        }
        #region comm.
        /*private static int InternalBinarySearch( IComparer< T > comparer, T[] array, int index, int length, T value )
        {
            var i = index;
            for ( var endIndex = index + length - 1; i <= endIndex; )
            {
                var middleIndex = i + ((endIndex - i) >> 1);
                var d = comparer.Compare( array[ middleIndex ], value );
                if ( d == 0 )
                {
                    return (middleIndex);
                }

                if ( d < 0 )
                {
                    i = middleIndex + 1;
                }
                else
                {
                    endIndex = middleIndex - 1;
                }
            }
            return (~i);
        }*/
        #endregion

        #region [.IEnumerable< T >.]
        public IEnumerator< T > GetEnumerator()
        {
            for ( var i = 0; i < _Size; i++ )
            {
                yield return (_Array[ i ]);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

//#if DEBUG
        public override string ToString() => (_Size == 0) ? "EMPTY" : $"item-count: {_Size}, min: '{_Array[ 0 ]}', max: '{_Array[ _Size - 1 ]}'";
//#endif
    }
}
