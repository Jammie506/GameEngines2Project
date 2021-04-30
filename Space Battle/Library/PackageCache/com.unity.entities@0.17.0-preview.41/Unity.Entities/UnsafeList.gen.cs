//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     TextTransform Samples/Packages/com.unity.entities/Unity.Entities/UnsafeList.tt
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Entities
{
    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}")]
    [DebuggerTypeProxy(typeof(UnsafeIntListDebugView))]
    internal unsafe struct UnsafeIntList
        : INativeDisposable
//        , INativeList<int>
    {
        [NativeDisableUnsafePtrRestriction]
        public readonly int* Ptr;
        public readonly int Length;
        public readonly int Capacity;
        public readonly AllocatorManager.AllocatorHandle Allocator;

        public unsafe UnsafeIntList(int initialCapacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.None; this.ListData() = new UnsafeList(UnsafeUtility.SizeOf<int>(), UnsafeUtility.AlignOf<int>(), initialCapacity, allocator, options); }
        public unsafe UnsafeIntList(int initialCapacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.None; this.ListData() = new UnsafeList(UnsafeUtility.SizeOf<int>(), UnsafeUtility.AlignOf<int>(), initialCapacity, allocator, options); }
        public bool IsEmpty => !IsCreated || Length == 0;
        public bool IsCreated => Ptr != null;
        public void Dispose() { this.ListData().Dispose(); }
        public JobHandle Dispose(JobHandle inputDeps) { return this.ListData().Dispose(inputDeps); }
        public void Clear() { this.ListData().Clear(); }
        public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { this.ListData().Resize<int>(length, options); }
        public void SetCapacity(int capacity) { this.ListData().SetCapacity<int>(capacity); }
        public void TrimExcess() { this.ListData().TrimExcess<int>(); }
        public int IndexOf(int value) { return this.ListData().IndexOf(value); }
        public bool Contains(int value) { return this.ListData().Contains(value); }
        public void AddNoResize(int value) { this.ListData().AddNoResize(value); }
        public void AddRangeNoResize(void* ptr, int length) { this.ListData().AddRangeNoResize<int>(ptr, length); }
        public void AddRangeNoResize(UnsafeIntList src) { this.ListData().AddRangeNoResize<int>(src.ListData()); }
        public void Add(in int value) { this.ListData().Add(value); }
        public void AddRange(UnsafeIntList src) { this.ListData().AddRange<int>(src.ListData()); }
        public void RemoveAtSwapBack(int index) { this.ListData().RemoveAtSwapBack<int>(index); }

        public ParallelReader AsParallelReader() { return new ParallelReader(Ptr, Length); }

        public unsafe struct ParallelReader
        {
            [NativeDisableUnsafePtrRestriction]
            public readonly int* Ptr;
            public readonly int Length;

            public ParallelReader(int* ptr, int length) { Ptr = ptr; Length = length; }
            public int IndexOf(int value) { for (int i = 0; i < Length; ++i) { if (Ptr[i] == value) { return i; } } return -1; }
            public bool Contains(int value) { return IndexOf(value) != -1; }
        }

        public ParallelWriter AsParallelWriter() { return new ParallelWriter((UnsafeList*)UnsafeUtility.AddressOf(ref this)); }

        public unsafe struct ParallelWriter
        {
            public UnsafeList.ParallelWriter Writer;

            internal unsafe ParallelWriter(UnsafeList* listData) { Writer = listData->AsParallelWriter(); }

            public void AddNoResize(int value) { Writer.AddNoResize(value); }
            public void AddRangeNoResize(void* ptr, int length) { Writer.AddRangeNoResize<int>(ptr, length); }
            public void AddRangeNoResize(UnsafeIntList list) { Writer.AddRangeNoResize<int>(UnsafeIntListExtensions.ListData(ref list)); }
        }
    }

    internal static class UnsafeIntListExtensions
    {
        public static ref UnsafeList ListData(ref this UnsafeIntList from) => ref UnsafeUtility.As<UnsafeIntList, UnsafeList>(ref from);
    }

    sealed class UnsafeIntListDebugView
    {
        private UnsafeIntList m_ListData;

        public UnsafeIntListDebugView(UnsafeIntList listData)
        {
            m_ListData = listData;
        }

        public unsafe int[] Items
        {
            get
            {
                var result = new int[m_ListData.Length];
                var ptr    = m_ListData.Ptr;

                for (int i = 0, num = result.Length; i < num; ++i)
                {
                    result[i] = ptr[i];
                }

                return result;
            }
        }
    }

    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}")]
    [DebuggerTypeProxy(typeof(UnsafeUintListDebugView))]
    internal unsafe struct UnsafeUintList
        : INativeDisposable
//        , INativeList<uint>
    {
        [NativeDisableUnsafePtrRestriction]
        public readonly uint* Ptr;
        public readonly int Length;
        public readonly int Capacity;
        public readonly AllocatorManager.AllocatorHandle Allocator;

        public unsafe UnsafeUintList(int initialCapacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.None; this.ListData() = new UnsafeList(UnsafeUtility.SizeOf<uint>(), UnsafeUtility.AlignOf<uint>(), initialCapacity, allocator, options); }
        public unsafe UnsafeUintList(int initialCapacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.None; this.ListData() = new UnsafeList(UnsafeUtility.SizeOf<uint>(), UnsafeUtility.AlignOf<uint>(), initialCapacity, allocator, options); }
        public bool IsEmpty => !IsCreated || Length == 0;
        public bool IsCreated => Ptr != null;
        public void Dispose() { this.ListData().Dispose(); }
        public JobHandle Dispose(JobHandle inputDeps) { return this.ListData().Dispose(inputDeps); }
        public void Clear() { this.ListData().Clear(); }
        public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { this.ListData().Resize<uint>(length, options); }
        public void SetCapacity(int capacity) { this.ListData().SetCapacity<uint>(capacity); }
        public void TrimExcess() { this.ListData().TrimExcess<uint>(); }
        public int IndexOf(uint value) { return this.ListData().IndexOf(value); }
        public bool Contains(uint value) { return this.ListData().Contains(value); }
        public void AddNoResize(uint value) { this.ListData().AddNoResize(value); }
        public void AddRangeNoResize(void* ptr, int length) { this.ListData().AddRangeNoResize<uint>(ptr, length); }
        public void AddRangeNoResize(UnsafeUintList src) { this.ListData().AddRangeNoResize<uint>(src.ListData()); }
        public void Add(in uint value) { this.ListData().Add(value); }
        public void AddRange(UnsafeUintList src) { this.ListData().AddRange<uint>(src.ListData()); }
        public void RemoveAtSwapBack(int index) { this.ListData().RemoveAtSwapBack<uint>(index); }

        public ParallelReader AsParallelReader() { return new ParallelReader(Ptr, Length); }

        public unsafe struct ParallelReader
        {
            [NativeDisableUnsafePtrRestriction]
            public readonly uint* Ptr;
            public readonly int Length;

            public ParallelReader(uint* ptr, int length) { Ptr = ptr; Length = length; }
            public int IndexOf(uint value) { for (int i = 0; i < Length; ++i) { if (Ptr[i] == value) { return i; } } return -1; }
            public bool Contains(uint value) { return IndexOf(value) != -1; }
        }

        public ParallelWriter AsParallelWriter() { return new ParallelWriter((UnsafeList*)UnsafeUtility.AddressOf(ref this)); }

        public unsafe struct ParallelWriter
        {
            public UnsafeList.ParallelWriter Writer;

            internal unsafe ParallelWriter(UnsafeList* listData) { Writer = listData->AsParallelWriter(); }

            public void AddNoResize(uint value) { Writer.AddNoResize(value); }
            public void AddRangeNoResize(void* ptr, int length) { Writer.AddRangeNoResize<uint>(ptr, length); }
            public void AddRangeNoResize(UnsafeUintList list) { Writer.AddRangeNoResize<uint>(UnsafeUintListExtensions.ListData(ref list)); }
        }
    }

    internal static class UnsafeUintListExtensions
    {
        public static ref UnsafeList ListData(ref this UnsafeUintList from) => ref UnsafeUtility.As<UnsafeUintList, UnsafeList>(ref from);
    }

    sealed class UnsafeUintListDebugView
    {
        private UnsafeUintList m_ListData;

        public UnsafeUintListDebugView(UnsafeUintList listData)
        {
            m_ListData = listData;
        }

        public unsafe uint[] Items
        {
            get
            {
                var result = new uint[m_ListData.Length];
                var ptr    = m_ListData.Ptr;

                for (int i = 0, num = result.Length; i < num; ++i)
                {
                    result[i] = ptr[i];
                }

                return result;
            }
        }
    }

    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}")]
    [DebuggerTypeProxy(typeof(UnsafeChunkPtrListDebugView))]
    internal unsafe struct UnsafeChunkPtrList
        : INativeDisposable
//        , INativeList<Chunk>
    {
        [NativeDisableUnsafePtrRestriction]
        public readonly Chunk** Ptr;
        public readonly int Length;
        public readonly int Capacity;
        public readonly AllocatorManager.AllocatorHandle Allocator;

        public unsafe UnsafeChunkPtrList(Chunk** ptr, int length) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.Invalid; this.ListData() = new UnsafePtrList((void**)ptr, length); }
        public unsafe UnsafeChunkPtrList(int initialCapacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.None; this.ListData() = new UnsafePtrList(initialCapacity, allocator, options); }
        public bool IsEmpty => !IsCreated || Length == 0;
        public bool IsCreated => Ptr != null;
        public void Dispose() { this.ListData().Dispose(); }
        public JobHandle Dispose(JobHandle inputDeps) { return this.ListData().Dispose(inputDeps); }
        public void Clear() { this.ListData().Clear(); }
        public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { this.ListData().Resize(length, options); }
        public void SetCapacity(int capacity) { this.ListData().SetCapacity(capacity); }
        public void TrimExcess() { this.ListData().TrimExcess(); }
        public int IndexOf(Chunk* value) { return this.ListData().IndexOf(value); }
        public bool Contains(Chunk* value) { return this.ListData().Contains(value); }
        public void Add(in Chunk* value) { this.ListData().Add(value); }
        public void AddRange(UnsafeChunkPtrList src) { this.ListData().AddRange(src.ListData()); }
        public void RemoveAtSwapBack(int index) { this.ListData().RemoveAtSwapBack(index); }

        public ParallelReader AsParallelReader() { return new ParallelReader(Ptr, Length); }

        public unsafe struct ParallelReader
        {
            [NativeDisableUnsafePtrRestriction]
            public readonly Chunk** Ptr;
            public readonly int Length;

            public ParallelReader(Chunk** ptr, int length) { Ptr = ptr; Length = length; }
            public int IndexOf(Chunk* value) { for (int i = 0; i < Length; ++i) { if (Ptr[i] == value) { return i; } } return -1; }
            public bool Contains(Chunk* value) { return IndexOf(value) != -1; }
        }

        public ParallelWriter AsParallelWriter() { return new ParallelWriter((UnsafePtrList*)UnsafeUtility.AddressOf(ref this)); }

        public unsafe struct ParallelWriter
        {
            public UnsafePtrList.ParallelWriter Writer;

            internal unsafe ParallelWriter(UnsafePtrList* listData) { Writer = listData->AsParallelWriter(); }

            public void AddNoResize(Chunk* value) { Writer.AddNoResize(value); }
            public void AddRangeNoResize(void** ptr, int length) { Writer.AddRangeNoResize(ptr, length); }
            public void AddRangeNoResize(UnsafeChunkPtrList list) { Writer.AddRangeNoResize((void**)list.Ptr, list.Length); }
        }
    }

    internal static class UnsafeChunkPtrListExtensions
    {
        public static ref UnsafePtrList ListData(ref this UnsafeChunkPtrList from) => ref UnsafeUtility.As<UnsafeChunkPtrList, UnsafePtrList>(ref from);
    }

    sealed class UnsafeChunkPtrListDebugView
    {
        private UnsafeChunkPtrList m_ListData;

        public UnsafeChunkPtrListDebugView(UnsafeChunkPtrList listData)
        {
            m_ListData = listData;
        }

        public unsafe Chunk[] Items
        {
            get
            {
                var result = new Chunk[m_ListData.Length];
                var ptr    = m_ListData.Ptr;

                for (int i = 0, num = result.Length; i < num; ++i)
                {
                    if (ptr[i] != null)
                    {
                        result[i] = *(Chunk*)ptr[i];
                    }
                }

                return result;
            }
        }
    }
    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}")]
    [DebuggerTypeProxy(typeof(UnsafeArchetypePtrListDebugView))]
    internal unsafe struct UnsafeArchetypePtrList
        : INativeDisposable
//        , INativeList<Archetype>
    {
        [NativeDisableUnsafePtrRestriction]
        public readonly Archetype** Ptr;
        public readonly int Length;
        public readonly int Capacity;
        public readonly AllocatorManager.AllocatorHandle Allocator;

        public unsafe UnsafeArchetypePtrList(Archetype** ptr, int length) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.Invalid; this.ListData() = new UnsafePtrList((void**)ptr, length); }
        public unsafe UnsafeArchetypePtrList(int initialCapacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.None; this.ListData() = new UnsafePtrList(initialCapacity, allocator, options); }
        public bool IsEmpty => !IsCreated || Length == 0;
        public bool IsCreated => Ptr != null;
        public void Dispose() { this.ListData().Dispose(); }
        public JobHandle Dispose(JobHandle inputDeps) { return this.ListData().Dispose(inputDeps); }
        public void Clear() { this.ListData().Clear(); }
        public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { this.ListData().Resize(length, options); }
        public void SetCapacity(int capacity) { this.ListData().SetCapacity(capacity); }
        public void TrimExcess() { this.ListData().TrimExcess(); }
        public int IndexOf(Archetype* value) { return this.ListData().IndexOf(value); }
        public bool Contains(Archetype* value) { return this.ListData().Contains(value); }
        public void Add(in Archetype* value) { this.ListData().Add(value); }
        public void AddRange(UnsafeArchetypePtrList src) { this.ListData().AddRange(src.ListData()); }
        public void RemoveAtSwapBack(int index) { this.ListData().RemoveAtSwapBack(index); }

        public ParallelReader AsParallelReader() { return new ParallelReader(Ptr, Length); }

        public unsafe struct ParallelReader
        {
            [NativeDisableUnsafePtrRestriction]
            public readonly Archetype** Ptr;
            public readonly int Length;

            public ParallelReader(Archetype** ptr, int length) { Ptr = ptr; Length = length; }
            public int IndexOf(Archetype* value) { for (int i = 0; i < Length; ++i) { if (Ptr[i] == value) { return i; } } return -1; }
            public bool Contains(Archetype* value) { return IndexOf(value) != -1; }
        }

        public ParallelWriter AsParallelWriter() { return new ParallelWriter((UnsafePtrList*)UnsafeUtility.AddressOf(ref this)); }

        public unsafe struct ParallelWriter
        {
            public UnsafePtrList.ParallelWriter Writer;

            internal unsafe ParallelWriter(UnsafePtrList* listData) { Writer = listData->AsParallelWriter(); }

            public void AddNoResize(Archetype* value) { Writer.AddNoResize(value); }
            public void AddRangeNoResize(void** ptr, int length) { Writer.AddRangeNoResize(ptr, length); }
            public void AddRangeNoResize(UnsafeArchetypePtrList list) { Writer.AddRangeNoResize((void**)list.Ptr, list.Length); }
        }
    }

    internal static class UnsafeArchetypePtrListExtensions
    {
        public static ref UnsafePtrList ListData(ref this UnsafeArchetypePtrList from) => ref UnsafeUtility.As<UnsafeArchetypePtrList, UnsafePtrList>(ref from);
    }

    sealed class UnsafeArchetypePtrListDebugView
    {
        private UnsafeArchetypePtrList m_ListData;

        public UnsafeArchetypePtrListDebugView(UnsafeArchetypePtrList listData)
        {
            m_ListData = listData;
        }

        public unsafe Archetype[] Items
        {
            get
            {
                var result = new Archetype[m_ListData.Length];
                var ptr    = m_ListData.Ptr;

                for (int i = 0, num = result.Length; i < num; ++i)
                {
                    if (ptr[i] != null)
                    {
                        result[i] = *(Archetype*)ptr[i];
                    }
                }

                return result;
            }
        }
    }
    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}")]
    [DebuggerTypeProxy(typeof(UnsafeEntityQueryDataPtrListDebugView))]
    internal unsafe struct UnsafeEntityQueryDataPtrList
        : INativeDisposable
//        , INativeList<EntityQueryData>
    {
        [NativeDisableUnsafePtrRestriction]
        public readonly EntityQueryData** Ptr;
        public readonly int Length;
        public readonly int Capacity;
        public readonly AllocatorManager.AllocatorHandle Allocator;

        public unsafe UnsafeEntityQueryDataPtrList(EntityQueryData** ptr, int length) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.Invalid; this.ListData() = new UnsafePtrList((void**)ptr, length); }
        public unsafe UnsafeEntityQueryDataPtrList(int initialCapacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { Ptr = null; Length = 0; Capacity = 0; Allocator = AllocatorManager.None; this.ListData() = new UnsafePtrList(initialCapacity, allocator, options); }
        public bool IsEmpty => !IsCreated || Length == 0;
        public bool IsCreated => Ptr != null;
        public void Dispose() { this.ListData().Dispose(); }
        public JobHandle Dispose(JobHandle inputDeps) { return this.ListData().Dispose(inputDeps); }
        public void Clear() { this.ListData().Clear(); }
        public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) { this.ListData().Resize(length, options); }
        public void SetCapacity(int capacity) { this.ListData().SetCapacity(capacity); }
        public void TrimExcess() { this.ListData().TrimExcess(); }
        public int IndexOf(EntityQueryData* value) { return this.ListData().IndexOf(value); }
        public bool Contains(EntityQueryData* value) { return this.ListData().Contains(value); }
        public void Add(in EntityQueryData* value) { this.ListData().Add(value); }
        public void AddRange(UnsafeEntityQueryDataPtrList src) { this.ListData().AddRange(src.ListData()); }
        public void RemoveAtSwapBack(int index) { this.ListData().RemoveAtSwapBack(index); }

        public ParallelReader AsParallelReader() { return new ParallelReader(Ptr, Length); }

        public unsafe struct ParallelReader
        {
            [NativeDisableUnsafePtrRestriction]
            public readonly EntityQueryData** Ptr;
            public readonly int Length;

            public ParallelReader(EntityQueryData** ptr, int length) { Ptr = ptr; Length = length; }
            public int IndexOf(EntityQueryData* value) { for (int i = 0; i < Length; ++i) { if (Ptr[i] == value) { return i; } } return -1; }
            public bool Contains(EntityQueryData* value) { return IndexOf(value) != -1; }
        }

        public ParallelWriter AsParallelWriter() { return new ParallelWriter((UnsafePtrList*)UnsafeUtility.AddressOf(ref this)); }

        public unsafe struct ParallelWriter
        {
            public UnsafePtrList.ParallelWriter Writer;

            internal unsafe ParallelWriter(UnsafePtrList* listData) { Writer = listData->AsParallelWriter(); }

            public void AddNoResize(EntityQueryData* value) { Writer.AddNoResize(value); }
            public void AddRangeNoResize(void** ptr, int length) { Writer.AddRangeNoResize(ptr, length); }
            public void AddRangeNoResize(UnsafeEntityQueryDataPtrList list) { Writer.AddRangeNoResize((void**)list.Ptr, list.Length); }
        }
    }

    internal static class UnsafeEntityQueryDataPtrListExtensions
    {
        public static ref UnsafePtrList ListData(ref this UnsafeEntityQueryDataPtrList from) => ref UnsafeUtility.As<UnsafeEntityQueryDataPtrList, UnsafePtrList>(ref from);
    }

    sealed class UnsafeEntityQueryDataPtrListDebugView
    {
        private UnsafeEntityQueryDataPtrList m_ListData;

        public UnsafeEntityQueryDataPtrListDebugView(UnsafeEntityQueryDataPtrList listData)
        {
            m_ListData = listData;
        }

        public unsafe EntityQueryData[] Items
        {
            get
            {
                var result = new EntityQueryData[m_ListData.Length];
                var ptr    = m_ListData.Ptr;

                for (int i = 0, num = result.Length; i < num; ++i)
                {
                    if (ptr[i] != null)
                    {
                        result[i] = *(EntityQueryData*)ptr[i];
                    }
                }

                return result;
            }
        }
    }
}
