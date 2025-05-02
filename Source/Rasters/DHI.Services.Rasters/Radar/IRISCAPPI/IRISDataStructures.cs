namespace DHI.Services.Rasters.Radar.IRISCAPPI
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class IRISDataStructures
    {
        public enum IRISProductTypes
        {
            UnknownProductType,
            IRISRawProduct,
            IRISRain1Product,
            IRISCappiProduct
        }

        // ------------------------------------------------------------------------------
        // DHI Solution Software - 2016
        // This unit declares a number of data structures for reading an interpretation
        // of Radar Image files produced by the IRIS Software from Vaisala, Finland.
        // DHI has made the code in this unit with the purpose of constructing conversion
        // routines for transfer of IRIS format Radar Data sets to the file formats used
        // by DHI Radar Software.
        // Details about the IRIS data format can be found in documentation from Vaisala
        // available at: ftp://ftp.sigmet.com/outgoing/manuals/
        // Look for the document: ./program/3data.pdf
        // References, like "(3.2.26)", in the code below is to sections in this document.
        // Coding by DHI, Kumar Shanmugasundaram, April 2016.
        // ------------------------------------------------------------------------------
        public const int IRISDataTypeDbDbz = 2;

        // -----------------------------------------------------------------------
        // C code for converting binary angle data format to Degrees
        // /* ------------------------------
        // * These functions convert a 16/32-bit binary angle into a positive
        // * real number in degrees.  Result is between 0 and +360.
        // */
        // double fPDegFromBin2( BIN2 ibinang_a )
        // {
        // double fresult = ((double)ibinang_a ) / (65536.0 / 360.0) ;
        // return( fresult ) ;
        // }
        // FLT8 fPDegFromBin4( BIN4 ibinang_a )
        // {
        // FLT8 fresult = ((FLT8)ibinang_a ) / (4294967296.0 / 360.0) ;
        // return( fresult ) ;
        // }
        // -----------------------------------------------------------------------
        public static double FpDegFromBin2Angle(ushort b2)
        {
            return b2/(65536.0/360.0);
        }

        public static double FpDegFromBin4Angle(int b4)
        {
            return b4/(4294967296.0/360.0);
        }

        public static int GetDataSizeFromDataTypeCode(int dataTypeConst)
        {
            // See (3.8), Table 3-6, in IRIS Programmer's Manual - Data Formats
            var result = 2;
            switch (dataTypeConst)
            {
                // Set as default
                // Modify the A .. B: 1 .. 7, 14, 16 .. 19, 25, 27, 32, 35, 38, 39, 46, 48, 50, 52, 55, 57
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 7:
                case 14:
                case 16:
                case 17:
                case 18:
                case 19:
                case 25:
                case 27:
                case 32:
                case 35:
                case 38:
                case 39:
                case 46:
                case 48:
                case 50:
                case 52:
                case 55:
                case 57:
                    result = 1;
                    break;
                // Modify the A .. B: 8 .. 13, 15, 20 .. 24, 26, 28, 33, 36, 37, 40 .. 45, 47, 49, 51, 53, 54, 56, 58
                case 8:
                case 13:
                case 15:
                case 20:
                case 26:
                case 28:
                case 33:
                case 36:
                case 37:
                case 40:
                case 47:
                case 49:
                case 51:
                case 53:
                case 54:
                case 56:
                case 58:
                    result = 2;
                    break;
            }
            return result;
        }

        public static DateTime IRISYMDsTimeToDateTime(IRISYmdsTime irisTime)
        {
            var T = DateTime.Now;
            if ((0 <= irisTime.SecAfterMidnigth) && (irisTime.SecAfterMidnigth < 86400))
            {
                var secondsOfHour = irisTime.SecAfterMidnigth;
                var hr = secondsOfHour/3600;
                secondsOfHour -= hr*3600;
                var mi = secondsOfHour/60;
                var se = secondsOfHour - mi*60;

                T = new DateTime(irisTime.Year, irisTime.Month, irisTime.Day).Add(new TimeSpan(hr, mi, se));
            }
            var result = T;
            return result;
        }

        public struct IRISStructureHeader
        {
            // (3.2.49)
            public short StructureIdentifier;
            public short FormatVersionNumber;
            public int NumberOfBytes;
            public short Reserved;
            public short Flags;

            public static IRISStructureHeader FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISStructureHeader))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISStructureHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISStructureHeader));
                handle.Free();
                return s;
            }
        }

        public struct IRISYmdsTime
        {
            // (3.2.79) ymds_time Structure
            public int SecAfterMidnigth;
            public ushort TimeInfo;
            public short Year;
            public short Month;
            public short Day;

            public static IRISYmdsTime FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISYmdsTime))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISYmdsTime)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISYmdsTime));
                handle.Free();
                return s;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct IRISColorScaleDef
        {
            // (3.2.6) color_scale_def Structure
            public uint iflags;
            public int istart;
            public int istep;
            public ushort icolcnt;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public ushort[] ilevel_seams;

            public static IRISColorScaleDef FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISColorScaleDef))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISColorScaleDef)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISColorScaleDef));
                handle.Free();
                return s;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IRISReadB
        {
            public byte data;

            public static IRISReadB FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISReadB))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISReadB)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISReadB));
                handle.Free();
                return s;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IRISReadW
        {
            public ushort data;

            public static IRISReadW FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISReadW))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISReadW)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISReadW));
                handle.Free();
                return s;
            }
        }

        public struct IRISCatchPsiStruct
        {
            // (3.2.3)
            public uint Flags;
            public uint HoursOfAccumulation;
            public int ThresholdOffset;
            // in 1/1000 or mm
            public int ThresholdFaction;
            // in 1/1000
            public char[] Rain1ProductToUse;
            // Name of RAIN1 Product to use
            public char[] CatchmentFileToUse;
            // Name of catchment file to use
            public uint SecOfAcc;
            public uint Rain1MinZ;
            public uint Rain1SpanSec;
            public uint AverageGaugeCorrectionFactor;

            public static IRISCatchPsiStruct FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISCatchPsiStruct))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISCatchPsiStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISCatchPsiStruct));
                handle.Free();
                return s;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IRISProductConfiguration
        {
            // (3.2.26)
            public IRISStructureHeader structure_header;
            public ushort product_type_code;
            public ushort scheduling_code;
            public int sec_skip_between_runs;
            public IRISYmdsTime time_of_product;
            public IRISYmdsTime time_of_ingest_sweep;
            public IRISYmdsTime time_of_ingest_file;
            public ushort spare1;
            public ushort spare2;
            public ushort spare3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] product_conf_file;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] task_name;
            public ushort flag_word;
            public int Xscale_cmperpix;
            public int Yscale_cmperpix;
            public int Zscale_cmperpix;
            public int Xdirection_arr_size;
            public int Ydirection_arr_size;
            public int Zdirection_arr_size;
            public int Xradar_location;
            public int Yradar_location;
            public int Zradar_location;
            public int MaxRangeCmVer20;
            public byte HydroClass;
            public byte spare4;
            public ushort data_type_generated;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] projection_name;
            public ushort data_type_input;
            public byte projection_type_code;
            public byte spare5;
            public short radial_smoother;
            public short run_times_product_conf;
            public int ZRrel_const;
            public int ZRrel_exp;
            public short Xdir_smoother;
            public short Ydir_smoother;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)] public char[] product_spec_info;
            //// Not a string. See (3.2.29) for details. Not implemented.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public char[] minor_task_siffixes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] QPE_algorithm_name;
            public IRISColorScaleDef color_scal_def;

            public static IRISProductConfiguration FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISProductConfiguration))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISProductConfiguration)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISProductConfiguration));
                handle.Free();
                return s;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IRISProductEnd
        {
            // (3.2.27)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public char[] site_name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public char[] IRISversion_product;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public char[] IRISversion_ingest;
            public IRISYmdsTime time_of_oldest_ingest_file;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)] public byte[] spare1;
            public short local_time_west_GMT_minutes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public char[] HW_name_ingest_data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public char[] site_name_ingest_data;
            public short recorde_time_west_GMT_minutes;
            public int latitude_of_radar;
            public int longitude_of_radar;
            public short ground_height;
            public short height_of_radar;
            public int PRF_htz;
            public int pulse_1_100_ms; //Pulse width in 1/100 of microseconds
            public ushort signal_process_type;
            public ushort tigger_rate_scheme;
            public short num_smaple_used;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] clutter_fname;
            public ushort linear_filter;
            public int wavelength;
            public int truncation_height;
            public int range_firstbin_cm;
            public int range_lastbin_cm;
            public int num_out_bin;
            public ushort flagword;
            public short num_ingest_files;
            public ushort polarization_type;
            public short io_cal_horizontal_pol; // in 1/100 dBm
            public short noise_cal_horizontal_pol; //Noise at calibration, horizontal pol, in 1/100 dBm
            public short radar_const_horizontal_pol; // Radar constant, horizontal pol, in 1/100 dB
            public ushort rec_bandwidht_khz; //Receiver bandwidth in kHz
            public short curr_noise_lvl_horizontal_pol; //Current noise level, horizontal pol, in 1/100 dBm
            public short curr_noise_lvl_vertical_pol; //Current noise level, vertical pol, in 1/100 dBm
            public short ldr_offset; // LDR offset, in 1/100 dB
            public short zdr_offset; //ZDR offset, in 1/100 dB
            public ushort TCF_Cal_flags;
            public ushort TCF_Cal_flags2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)] public byte[] sparearr;
            public int standard_parallel1;
            public int standard_parallel2;
            public uint equatorial_radius; // of the earth, cm (zero = 6371km sphere)
            public uint flattening; //1/Flattening in 1/1000000 (zero = sphere)
            public uint fault_status_tast; //Fault status of task, see ingest_configuration 3.2.17 for details
            public uint mask_input_site; //Mask of input sites used in a composite
            public ushort num_log_filter; // Number of log based filter for the first bin
            public ushort cluttermap_ingest_data; // Nonzero if cluttermap applied to the ingest data
            public int latitude_proj; //Latitude of projection reference *
            public int longitude_proj; //Longitude of projection reference *
            public short product_sequence_number; //Product sequence number
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] sparearr2;
            public short melting_level; // Melting level in meters, msb complemented (0=unknown)
            public short radar_height_above_ref; // Height of radar above reference height in meters
            public short number_of_result_elements; //Number of elements in product results array
            public byte mean_wind_speed; // Mean wind speed
            public byte mean_wind_direction; //Mean wind direction (unknown if speed and direction 0)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] sparearr3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public char[] TZ_name_recorde_data; //TZ Name of recorded data
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] sparearr4;

            public static IRISProductEnd FromBinaryReaderBlock(BinaryReader br)
            {
                var buff = br.ReadBytes(Marshal.SizeOf(typeof (IRISProductEnd))); //faster than (Marshal.SizeOf(typeof(TestStruct)));
                var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                var s = (IRISProductEnd)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (IRISProductEnd));
                handle.Free();
                return s;
            }
        }
    }
}