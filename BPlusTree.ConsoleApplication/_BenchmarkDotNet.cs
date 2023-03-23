using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;

namespace BPlusTree.ConsoleApplication
{
    public abstract class __base< T >
    {
        [Params(100_000)]//1_000, 10_000, 25_000)]
        public virtual int COUNT { get; set; }

        [Params(64, 128, 256, 512, 1024)]
        public virtual int BLOCK_CAPACITY { get; set; }

        [Params(64, 128, 256, 512, 1024)]
        public virtual int BLOCK_LIST_CAPACITY { get; set; }

        protected abstract IComparer< T > _Comparer { get; }
    }

    public abstract class __string : __base< string >
    {
        protected void Fill( IBPlusTree< string > bptree, int count )
        {
            var rnd = new Random( 42 );
            //var sum = 0;
            for ( int i = 1; i <= count; i++ )
            {
                var s = rnd.Next().ToString(); // i.ToString();
                bptree.TryAdd( s );
                //sum += bptree.Contains( s ) ? 1 : 0;
            }
            //return (sum);
        }
        protected void Fill( ICollection< string > col, int count )
        {
            var rnd = new Random( 42 );
            //var sum = 0;
            for ( int i = 1; i <= count; i++ )
            {
                var s = rnd.Next().ToString(); // i.ToString();
                col.Add( s );
                //sum += bptree.Contains( s ) ? 1 : 0;
            }
            //return (sum);
        }

        protected override IComparer< string > _Comparer => StringComparer.Ordinal /*_StringComparer.Inst*/;
    }

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class _BPlusTreeList__string : __string
    {
        private void BPlusTreeList( int blockCapacity, int? blockListCapacity = null ) // (int) Math.Sqrt( COUNT );
        {
            if ( !blockListCapacity.HasValue )
            {
                blockListCapacity = ((int) (COUNT / blockCapacity * 1.0 + 0.5) + 25);
            }
            var bptree_lst = new BPlusTreeList< string >( _Comparer, blockListCapacity.Value, blockCapacity );

            Fill( bptree_lst, COUNT );
        }

        [Benchmark] public void Run() => BPlusTreeList( BLOCK_CAPACITY, BLOCK_LIST_CAPACITY );
    }

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class _BPlusTreeSet__string : __string
    {
        [Benchmark] public void Run()
        {
            var bptree_set = new BPlusTreeSet< string >( _Comparer, BLOCK_CAPACITY );

            Fill( bptree_set, COUNT );
        }
    }


    public abstract class __int : __base< int >
    {
        protected void Fill( IBPlusTree< int > bptree, int count )
        {
            var rnd = new Random( 42 );
            //var sum = 0;
            for ( int i = 1; i <= count; i++ )
            {
                var s = rnd.Next(); // i;
                bptree.TryAdd( s );
                //sum += bptree.Contains( s ) ? 1 : 0;
            }
            //return (sum);
        }
        protected void Fill( ICollection<int> col, int count )
        {
            var rnd = new Random( 42 );
            //var sum = 0;
            for ( int i = 1; i <= count; i++ )
            {
                var s = rnd.Next(); // i;
                col.Add( s );
                //sum += bptree.Contains( s ) ? 1 : 0;
            }
            //return (sum);
        }

        protected override IComparer< int > _Comparer => Int32Comparer.Inst;
    }

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class _BPlusTreeList__int : __int
    {
        private void BPlusTreeList( int blockCapacity, int? blockListCapacity = null ) // (int) Math.Sqrt( COUNT );
        {
            if ( !blockListCapacity.HasValue )
            {
                blockListCapacity = ((int) (COUNT / blockCapacity * 1.0 + 0.5) + 25);
            }
            var bptree_lst = new BPlusTreeList< int >( _Comparer, blockListCapacity.Value, blockCapacity );

            Fill( bptree_lst, COUNT );
        }

        [Benchmark] public void Run() => BPlusTreeList( BLOCK_CAPACITY, BLOCK_LIST_CAPACITY );
    }

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class _BPlusTreeSet__int : __int
    {
        [Benchmark] public void Run()
        {
            var bptree_set = new BPlusTreeSet<int>( _Comparer, BLOCK_CAPACITY );

            Fill( bptree_set, COUNT );
        }
    }
    //------------------------------------------------------------------------------------//


    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class BPlusTreeList_vs_SortedSet__int
    {
        private IComparer< int > _Comparer;

        [Params(250_000, 500_000)]//1_000, 10_000, 25_000)]
        public int COUNT;

        [Params(64)]
        public int BLOCK_CAPACITY;

        [Params(2048)]
        public int BLOCK_LIST_CAPACITY;

        [GlobalSetup]
        public void Setup() => _Comparer  = Int32Comparer.Inst;

        private static void FillCollection( ICollection< int > coll, int count  )
        {
            //---var sum = 0;
            var rnd = new Random( 42 );
            for ( ; 0 <= count; count-- )
            {
                coll.Add( rnd.Next() );
                //---sum += coll.Contains( s ) ? 1 : 0;
            }
            //---return (sum);
        }

        [Benchmark] public int SortedSet()
        {
            var set = new SortedSet< int >( _Comparer );

            FillCollection( set, COUNT );
            //---lst.Sort( _Int32Comparer );

            return (set.Count);
        }
        [Benchmark] public int BPlusTreeList()
        {
            var bptree = new BPlusTreeList< int >( _Comparer, BLOCK_LIST_CAPACITY, BLOCK_CAPACITY );

            FillCollection( bptree, COUNT );

            return (bptree.Count);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class BPlusTreeList_vs_List__int
    {
        private IComparer< int > _Comparer;

        [Params(1_000, 10_000, 25_000)]
        public int COUNT;

        [Params(512)]
        public int BLOCK_CAPACITY;

        //[Params(/*2048*/)]
        //public int BLOCK_LIST_CAPACITY;
        public int BLOCK_LIST_CAPACITY => ((int) (COUNT / BLOCK_CAPACITY * 1.0 + 0.5) + 25);

        [GlobalSetup]
        public void Setup() => _Comparer = Int32Comparer.Inst;

        private static void FillCollection( ICollection< int > coll, int count )
        {
            var rnd = new Random( 42 );
            for ( ; 0 <= count; count-- )
            {
                coll.Add( rnd.Next() );
            }
        }
        private static void FillList( List< int > lst, IComparer< int > comparer, int count )
        {
            var rnd = new Random( 42 );
            lst.Add( rnd.Next() );
            for ( ; 0 < count; count-- )
            {
                var n = rnd.Next();
                var last = lst[ lst.Count - 1 ];
                if ( n < last )
                {
                    lst.Add( n );
                    lst.Sort( comparer );
                }
                else
                {
                    lst.Add( n );
                }
            }
        }

        [Benchmark] public int List()
        {
            var lst = new List< int >();

            FillList( lst, _Comparer, COUNT );
            //---lst.Sort( _Comparer );

            return (lst.Count);
        }
        [Benchmark] public int BPlusTreeList()
        {
            var bptree = new BPlusTreeList< int >( _Comparer, BLOCK_LIST_CAPACITY, BLOCK_CAPACITY );

            FillCollection( bptree, COUNT );

            return (bptree.Count);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class BPlusTreeList_vs_SortedSet__string
    {
        private IComparer< string > _Comparer;

        [Params(250_000, 500_000)]//1_000, 10_000, 25_000)]
        public int COUNT;

        [Params(512)]
        public int BLOCK_CAPACITY;

        //[Params(/*2048*/)]
        //public int BLOCK_LIST_CAPACITY;
        public int BLOCK_LIST_CAPACITY => ((int) (COUNT / BLOCK_CAPACITY * 1.0 + 0.5) + 25);

        [GlobalSetup]
        public void Setup() => _Comparer = _StringComparer.Inst; //StringComparer.Ordinal;

        private static void FillCollection( ICollection< string > coll, int count )
        {
            //---var sum = 0;
            for ( int i = 1; i <= count; i++ )
            {
                var s = i.ToString();
                coll.Add( s );
                //---sum += coll.Contains( s ) ? 1 : 0;
            }
            //---return (sum);
        }

        [Benchmark] public int SortedSet()
        {
            var set = new SortedSet< string >( _Comparer );

            FillCollection( set, COUNT );

            return (set.Count);
        }
        [Benchmark] public int BPlusTreeList()
        {
            var bptree = new BPlusTreeList< string >( _Comparer, BLOCK_LIST_CAPACITY, BLOCK_CAPACITY );

            FillCollection( bptree, COUNT );

            return (bptree.Count);
        }
    }
    //------------------------------------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    public class BPlusTree__strings
    {
        private IComparer< string > _Comparer;
        private List< string > _SourceList;

        public BPlusTree__strings() { }
        public BPlusTree__strings( int count ) 
        {
            COUNT = count;
            Setup();
        }

        [GlobalSetup]
        public void Setup()
        {
            _Comparer = _StringComparer.Inst; //StringComparer.Ordinal;

            var rnd = new Random( 7 );
            int  funcGetCharCount() => rnd.Next( 5, 10 + 1 );
            char funcGetChar     () => (char) rnd.Next( 'A', 'Z' + 1 ); // (char) rnd.Next( '0', '9' + 1 ); //
            var buf = new StringBuilder();

            const int ITEMS_COUNT = 500_000;
            _SourceList = new List< string >( ITEMS_COUNT );
            
            for ( int i = 0; i < ITEMS_COUNT; i++ )
            {
                buf.Clear();
                for ( var n = funcGetCharCount(); 0 <= n; n-- )
                {
                    buf.Append( funcGetChar() );
                }
                _SourceList.Add( buf.ToString() );
            }
        }

        [Params(10_000/*500_000*/)]
        public int COUNT;

        [Params(256)]
        public int BLOCK_CAPACITY;

        private void FillCollection( ICollection< string > coll, int count )
        {
            for ( var i = 0; i < count; i++ )
            {
                coll.Add( _SourceList[ i ] );
            }
        }
        private void FillCollection< T >( ICollection< KeyValuePair< string, T > > coll, int count )
        {
            for ( var i = 0; i < count; i++ )
            {
                coll.Add( new KeyValuePair< string, T >( _SourceList[ i ], default ) );
            }
        }
        private void FillList( List< string > lst, IComparer< string > comparer, int count )
        {
            for ( var i = 0; i < count; i++ )
            {
                var s = _SourceList[ i ];
                var idx = lst.BinarySearch( s, comparer );
                if ( idx < 0 )
                {
                    var pos = ~idx;
                    lst.Insert( pos, s );
                }
            }

            var copy_lst = lst.ToList();
                copy_lst.Sort( comparer );
            var suc = copy_lst.SequenceEqual( lst );
            Debug.Assert( suc );
            if ( !suc )
            {
                throw (new InvalidOperationException());
            }
        }

        [Benchmark(Description=nameof(List))] public int List()
        {
            var list = new List< string >( COUNT );

            FillList( list, _Comparer, COUNT );

            return (list.Count);
        }
        [Benchmark(Description=nameof(SortedList))] public int SortedList()
        {
            var slist = new SortedList< string, int >( COUNT, _Comparer );

            FillCollection( slist, COUNT );

            return (slist.Count);
        }
        [Benchmark(Description=nameof(BPlusTreeList))] public int BPlusTreeList()
        {
            var bptree = BPlusTreeList< string >.Create( _Comparer, COUNT, BLOCK_CAPACITY );

            FillCollection( bptree, COUNT );

            return (bptree.Count);
        }
        [Benchmark(Description=nameof(SortedSet))] public int SortedSet()
        {
            var set = new SortedSet< string >( _Comparer );

            FillCollection( set, COUNT );

            return (set.Count);
        }
        [Benchmark(Description=nameof(BPlusTreeSet))] public int BPlusTreeSet()
        {
            var bptree = new BPlusTreeSet< string >( _Comparer, BLOCK_CAPACITY );

            FillCollection( bptree, COUNT );

            return (bptree.Count);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SimpleJob]
    [RPlotExporter, RankColumn]
    [MemoryDiagnoser]
    public class BPlusTreeSet__strings__MemoryDiagnoser
    {
        private IComparer< string > _Comparer;
        private List< string > _SourceList;

        public BPlusTreeSet__strings__MemoryDiagnoser() { }
        public BPlusTreeSet__strings__MemoryDiagnoser( int count ) 
        {
            COUNT = count;
            Setup();
        }

        [GlobalSetup]
        public void Setup()
        {
            _Comparer = _StringComparer.Inst; //StringComparer.Ordinal;

            var rnd = new Random( 7 );
            int  funcGetCharCount() => rnd.Next( 5, 10 + 1 );
            char funcGetChar     () => (char) rnd.Next( 'A', 'Z' + 1 ); // (char) rnd.Next( '0', '9' + 1 ); //
            var buf = new StringBuilder();

            const int ITEMS_COUNT = 500_000;
            _SourceList = new List< string >( ITEMS_COUNT );
            
            for ( int i = 0; i < ITEMS_COUNT; i++ )
            {
                buf.Clear();
                for ( var n = funcGetCharCount(); 0 <= n; n-- )
                {
                    buf.Append( funcGetChar() );
                }
                _SourceList.Add( buf.ToString() );
            }
        }

        [Params(/*10_000*/500_000)]
        public int COUNT;

        [Params(256)]
        public int BLOCK_CAPACITY;

        private void FillCollection( ICollection< string > coll, int count )
        {
            for ( var i = 0; i < count; i++ )
            {
                coll.Add( _SourceList[ i ] );
            }
        }
        private void FillCollection< T >( ICollection< KeyValuePair< string, T > > coll, int count )
        {
            for ( var i = 0; i < count; i++ )
            {
                coll.Add( new KeyValuePair< string, T >( _SourceList[ i ], default ) );
            }
        }
        private void FillList( List< string > lst, IComparer< string > comparer, int count )
        {
            for ( var i = 0; i < count; i++ )
            {
                var s = _SourceList[ i ];
                var idx = lst.BinarySearch( s, comparer );
                if ( idx < 0 )
                {
                    var pos = ~idx;
                    lst.Insert( pos, s );
                }
            }

            var copy_lst = lst.ToList();
                copy_lst.Sort( comparer );
            var suc = copy_lst.SequenceEqual( lst );
            Debug.Assert( suc );
            if ( !suc )
            {
                throw (new InvalidOperationException());
            }
        }

        /*
        [Benchmark(Description=nameof(List))] public int List()
        {
            var list = new List< string >( COUNT );

            FillList( list, _Comparer, COUNT );

            return (list.Count);
        }
        [Benchmark(Description=nameof(SortedList))] public int SortedList()
        {
            var slist = new SortedList< string, int >( COUNT, _Comparer );

            FillCollection( slist, COUNT );

            return (slist.Count);
        }
        [Benchmark(Description=nameof(BPlusTreeList))] public int BPlusTreeList()
        {
            var bptree = BPlusTreeList< string >.Create( _Comparer, COUNT, BLOCK_CAPACITY );

            FillCollection( bptree, COUNT );

            return (bptree.Count);
        }
        //*/

        [Benchmark(Description=nameof(SortedSet))] public int SortedSet()
        {
            var set = new SortedSet< string >( _Comparer );

            FillCollection( set, COUNT );

            return (set.Count);
        }
        [Benchmark(Description=nameof(BPlusTreeSet))] public int BPlusTreeSet()
        {
            var bptree = new BPlusTreeSet< string >( _Comparer, BLOCK_CAPACITY );

            FillCollection( bptree, COUNT );

            return (bptree.Count);
        }
    }
}
