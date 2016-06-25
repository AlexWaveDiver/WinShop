using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace WinShop
{
    public class Packing
    {
        static BinaryWriter bwrite;
        static BinaryReader bread;
        static uint form_size;
        static uint FONT_offset;
        static string[] chunks = new string[] { "GEN8","OPTN","EXTN","SOND","AGRP","SPRT","BGND","PATH","SCPT","SHDR","FONT","TMLN","OBJT","ROOM","DAFL","TPAG","CODE","VARI",
                                    "FUNC","STRG","TXTR","AUDO" };


        public static void Pack(string sourceFolder, string winFile)
        {
            string output_folder = sourceFolder;
            if (output_folder[output_folder.Length-1]!='\\') output_folder += '\\';
            string input_win = winFile;
            form_size = 0;

            try
            {
                if (bwrite.GetType() == typeof(BinaryWriter))
                {
                    bwrite.Close();
                }
            } catch (Exception e) {}

            bwrite = new BinaryWriter(File.Open(input_win, FileMode.Create));
            bwrite.Write(System.Text.Encoding.ASCII.GetBytes("FORM"));
            bwrite.Write(form_size);

            for (int i = 0; i < chunks.Length; i++)
            {
                string chunk_name = chunks[i];
                uint chunk_size = 0;
                bwrite.Write(System.Text.Encoding.ASCII.GetBytes(chunk_name));
                uint chunk_offset = (uint)bwrite.BaseStream.Position;
                bwrite.Write(chunk_size);
                form_size += 8;

                if (chunk_name == "STRG")
                {
                    string[] strg = File.ReadAllLines(output_folder + "STRG.txt", System.Text.Encoding.UTF8);
                    uint lines = (uint)strg.Length;
                    if (strg[(int)(lines - 1)].Length == 0) lines--;

                    bwrite.Write(lines);
                    chunk_size += 4;

                    uint[] Offsets = new uint[lines];
                    //Lines offsets
                    for (int f = 0; f < lines; f++)
                    {
                        Offsets[f] = (uint)bwrite.BaseStream.Position;
                        bwrite.Write((uint)0);
                        chunk_size += 4;
                    }
                    //Lines (line size + line + 0)
                    for (int f = 0; f < lines; f++)
                    {
                        uint line_off = (uint)bwrite.BaseStream.Position;
                        bwrite.BaseStream.Position = Offsets[f];
                        bwrite.Write(line_off);
                        bwrite.BaseStream.Position = line_off;

                        string oneLine = strg[f];
                        uint lineLen = (uint)oneLine.Length;
                        bwrite.Write(lineLen); chunk_size += 4;
                        for (int j = 0; j < lineLen; j++)
                            bwrite.Write(oneLine[j]);
                        chunk_size += (uint)System.Text.Encoding.UTF8.GetByteCount(oneLine);
                        bwrite.Write((byte)0); chunk_size += 1;
                    }

                    //Edited lines                    
                    if (File.Exists(output_folder + "translate.txt"))
                    {
                        FileInfo finfo = new FileInfo(output_folder + "translate.txt");
                        if (finfo.Length > 0)
                        {
                            string[] patch = File.ReadAllLines(output_folder + "translate.txt", System.Text.Encoding.UTF8);
                            lines = (uint)patch.Length;

                            if (lines > 0)
                            {
                                if (patch[(int)(lines - 1)].Length == 0) lines--;
                                bool patchNumber = true;
                                for (int f = 0; f < lines; f++)
                                {
                                    string oneLine = patch[f];
                                    if (oneLine.IndexOf("//") == 0) continue;

                                    if (patchNumber)
                                    {
                                        uint lineN = System.Convert.ToUInt32(oneLine);
                                        uint line_offset = chunk_offset + (lineN + 1) * 4;

                                        uint line_off = (uint)bwrite.BaseStream.Position;
                                        bwrite.BaseStream.Position = line_offset;
                                        bwrite.Write(line_off);
                                        bwrite.BaseStream.Position = line_off;
                                    }
                                    else
                                    {
                                        uint lineLen = (uint)oneLine.Length;
                                        bwrite.Write(lineLen); chunk_size += 4;
                                        for (int j = 0; j < lineLen; j++)
                                            bwrite.Write(oneLine[j]);
                                        chunk_size += (uint)System.Text.Encoding.UTF8.GetByteCount(oneLine);
                                        bwrite.Write((byte)0); chunk_size += 1;
                                    }
                                    patchNumber = !patchNumber;
                                }
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("translate.txt empty. Strings will not be modified.");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("translate.txt not found. Strings will not be modified.");
                    }

                    //Font patching                  
                    string patch_path = output_folder + "patch\\";
                    if (File.Exists(patch_path + "patch.txt"))
                    {
                        FileInfo finfo = new FileInfo(patch_path + "patch.txt");
                        if (finfo.Length > 0)
                        {
                            StreamReader patchText = new StreamReader(patch_path + "patch.txt", System.Text.Encoding.ASCII);
                            for (string oneLine = patchText.ReadLine(); oneLine != null; oneLine = patchText.ReadLine())
                            {
                                if (oneLine.IndexOf("//") == 0) continue;
                                string[] par = oneLine.Split(';');

                                int index_replaced = Convert.ToInt32(par[0]);
                                string old_font_name = par[1];
                                string new_font_name = par[2];
                                ushort x = Convert.ToUInt16(par[3]);
                                ushort y = Convert.ToUInt16(par[4]);
                                ushort w = Convert.ToUInt16(par[5]);
                                ushort h = Convert.ToUInt16(par[6]);
                                ushort s = Convert.ToUInt16(par[7]);

                                uint bacp = (uint)bwrite.BaseStream.Position;
                                bwrite.BaseStream.Position = FONT_offset + 4 * (index_replaced + 1);//!!!
                                bwrite.Write(bacp);
                                bwrite.BaseStream.Position = bacp;

                                //Preparing font
                                uint font_size = (uint)new FileInfo(patch_path + "\\" + new_font_name).Length;
                                BinaryReader new_font_file = new BinaryReader(File.Open(patch_path + "\\" + new_font_name, FileMode.Open));
                                BinaryReader old_font_file = new BinaryReader(File.Open(patch_path + "\\" + old_font_name, FileMode.Open));

                                for (uint j = 0; j < 8; j++)
                                    bwrite.Write(old_font_file.ReadByte());//Name and font family
                                new_font_file.BaseStream.Position += 8;
                                for (uint j = 0; j < 20; j++)
                                    bwrite.Write(new_font_file.ReadByte());
                                old_font_file.BaseStream.Position += 20;
                                uint sprite_offset = old_font_file.ReadUInt32();

                                editSprite(sprite_offset, x, y, w, h, s);//!!!

                                bwrite.Write(sprite_offset);
                                new_font_file.BaseStream.Position += 4;
                                for (uint j = 0; j < 8; j++)
                                    bwrite.Write(new_font_file.ReadByte());
                                uint glyph_count = new_font_file.ReadUInt32();
                                bwrite.Write(glyph_count);

                                for (uint j = 0; j < glyph_count; j++)
                                    bwrite.Write((uint)(bwrite.BaseStream.Position + (glyph_count - j) * 4 + j * 16));
                                new_font_file.BaseStream.Position += glyph_count * 4;
                                for (uint j = 0; j < (glyph_count) * 16; j++)
                                    bwrite.Write(new_font_file.ReadByte());

                                chunk_size += font_size;
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("patch.txt empty. Fonts will not be modified.");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("patch.txt not found. Fonts will not be modified.");
                    }

                    bwrite.Write((uint)0); chunk_size += 4;
                }
                else if (chunk_name == "TXTR")
                {
                    uint files = (uint)Directory.GetFiles(output_folder + chunk_name).Length;
                    bwrite.Write(files);
                    chunk_size += 4;

                    uint[] Offsets = new uint[files];

                    //Headers offset
                    for (int f = 0; f < files; f++)
                    {
                        Offsets[f] = (uint)bwrite.BaseStream.Position;
                        bwrite.Write((uint)0);
                        chunk_size += 4;
                    }

                    //Headers
                    for (int f = 0; f < files; f++)
                    {
                        uint header_off = (uint)bwrite.BaseStream.Position;
                        bwrite.BaseStream.Position = Offsets[f];
                        bwrite.Write(header_off);
                        bwrite.BaseStream.Position = header_off;

                        bwrite.Write((uint)1);
                        Offsets[f] = (uint)bwrite.BaseStream.Position;
                        bwrite.Write((uint)0);
                        chunk_size += 8;
                    }

                    //Some zeros are added on packaging, I don't know why but
                    //the game still runs without them.
                    //for (int f=0; f<13; f++) 
                    //{
                    //    bwrite.Write((uint)0); chunk_size += 4;
                    //}

                    //Files
                    for (uint f0 = 0; f0 < files; f0++)
                    {
                        uint file_off = (uint)bwrite.BaseStream.Position;
                        bwrite.BaseStream.Position = Offsets[f0];
                        bwrite.Write(file_off);
                        bwrite.BaseStream.Position = file_off;

                        uint file_size = (uint)new FileInfo(output_folder + chunk_name + "\\" + f0 + ".png").Length;
                        bread = new BinaryReader(File.Open(output_folder + chunk_name + "\\" + f0 + ".png", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                        for (uint j = 0; j < file_size; j++)
                            bwrite.Write(bread.ReadByte());
                        chunk_size += file_size;
                    }
                }
                else if (chunk_name == "AUDO")
                {
                    uint files = (uint)Directory.GetFiles(output_folder + chunk_name).Length;
                    bwrite.Write(files); chunk_size += 4;

                    uint[] Offsets = new uint[files];

                    //Headers offset
                    for (int f = 0; f < files; f++)
                    {
                        Offsets[f] = (uint)bwrite.BaseStream.Position;
                        bwrite.Write((uint)0);
                        chunk_size += 4;
                    }

                    for (uint f0 = 0; f0 < files; f0++)
                    {
                        uint file_off = (uint)bwrite.BaseStream.Position;
                        bwrite.BaseStream.Position = Offsets[f0];
                        bwrite.Write(file_off);
                        bwrite.BaseStream.Position = file_off;

                        uint file_size = (uint)new FileInfo(output_folder + chunk_name + "\\" + f0 + ".wav").Length;
                        bwrite.Write(file_size); chunk_size += 4;
                        bread = new BinaryReader(File.Open(output_folder + chunk_name + "\\" + f0 + ".wav", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                        for (uint j = 0; j < file_size; j++)
                            bwrite.Write(bread.ReadByte());
                        chunk_size += file_size;
                        if (f0 == files - 1) continue;
                        for (int j = 0; j < file_size % 4; j++)
                        { bwrite.Write((byte)0); chunk_size++; }
                    }
                }
                else
                {
                    string filer = output_folder + "CHUNK\\" + chunk_name + ".chunk";
                    if (chunk_name == "FONT") FONT_offset = (uint)bwrite.BaseStream.Position;
                    chunk_size = (uint)new FileInfo(filer).Length;
                    bread = new BinaryReader(File.Open(filer, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                    for (uint j = 0; j < chunk_size; j++)
                        bwrite.Write(bread.ReadByte());
                    bread.Close();
                }

                uint chunk_end = (uint)bwrite.BaseStream.Position;
                bwrite.BaseStream.Position = chunk_offset;
                bwrite.Write(chunk_size);
                bwrite.BaseStream.Position = chunk_end;
                form_size += chunk_size;

                System.Console.WriteLine("Chunk " + chunk_name + " offset:" + (chunk_offset - 4) + " size:" + (chunk_size + 8));
            }

            bwrite.BaseStream.Position = 4;
            bwrite.Write(form_size);
            bwrite.Close();
            bread.Close();
            Console.WriteLine("DONE");
        }

        static void editSprite(uint sprite_offset, ushort x, ushort y, ushort w, ushort h, ushort s)
        {
            uint bacp = (uint)bwrite.BaseStream.Position;
            bwrite.BaseStream.Position = sprite_offset;
            bwrite.Write(x);
            bwrite.Write(y);
            bwrite.Write(w);
            bwrite.Write(h);
            bwrite.BaseStream.Position += 4;
            bwrite.Write(w);
            bwrite.Write(h);
            bwrite.Write(w);
            bwrite.Write(h);
            bwrite.Write(s);
            bwrite.BaseStream.Position = bacp;
        }
    }
    public class Unpacking
    {
        static BinaryReader bread;
        static BinaryWriter bwrite;
        static string input_folder;
        static uint chunk_limit;
        static uint FONT_offset;
        static uint FONT_limit;
        static uint STRG_offset;

        struct endFiles
        {
            public string name;
            public uint offset;
            public uint size;
        }

        struct spriteInfo
        {
            public uint x;
            public uint y;
            public uint w;
            public uint h;
            public uint i;
        }

        public static void Unpack(string winFile, string destFolder)
        {
            string output_win = winFile;
            input_folder = destFolder;
            if (input_folder[input_folder.Length - 1] != '\\') input_folder += '\\';
            uint full_size = (uint)new FileInfo(output_win).Length;
            bread = new BinaryReader(File.Open(output_win, FileMode.Open));
            Directory.CreateDirectory(input_folder + "CHUNK");

            uint chunk_offset = 0;

            while (chunk_offset < full_size)
            {
                string chunk_name = new String(bread.ReadChars(4));
                uint chunk_size = bread.ReadUInt32();
                chunk_offset = (uint)bread.BaseStream.Position;
                chunk_limit = chunk_offset + chunk_size;
                System.Console.WriteLine("Chunk " + chunk_name + " offset:" + chunk_offset + " size:" + chunk_size);

                List<endFiles> filesToCreate = new List<endFiles>();

                if (chunk_name == "FORM")
                {
                    full_size = chunk_limit;
                    chunk_size = 0;
                }
                else if (chunk_name == "TPAG")
                {
                    //StreamWriter tpag = new StreamWriter(input_folder + "TPAG.txt", false, System.Text.Encoding.ASCII);
                    //uint sprite_count = bread.ReadUInt32();
                    //bread.BaseStream.Position += sprite_count * 4;//Skip offsets
                    //for (uint i = 0; i < sprite_count; i++)
                    //{
                    //    tpag.Write(bread.ReadInt16());//x
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//y
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//w1
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//h1
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//?
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//?
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//w2
                    //    tpag.Write(";");                        
                    //    tpag.Write(bread.ReadInt16());//h2
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//w3
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//h3
                    //    tpag.Write(";");
                    //    tpag.Write(bread.ReadInt16());//txtr id
                    //    tpag.Write((char)0x0D);
                    //    tpag.Write((char)0x0A);
                    //}
                    //bread.BaseStream.Position = chunk_offset;
                }
                else if (chunk_name == "STRG")
                {
                    STRG_offset = (uint)bread.BaseStream.Position;
                    bwrite = new BinaryWriter(File.Open(input_folder + "STRG.txt", FileMode.Create));
                    uint strings = bread.ReadUInt32();
                    bread.BaseStream.Position += strings * 4;//Skip offsets
                    for (uint i = 0; i < strings; i++)
                    {
                        uint string_size = bread.ReadUInt32() + 1;
                        for (uint j = 0; j < string_size; j++)
                            bwrite.Write(bread.ReadByte());
                        bwrite.BaseStream.Position--;
                        bwrite.Write((byte)0x0D);
                        bwrite.Write((byte)0x0A);
                    }
                    bwrite.Close();
                    long bacp = bread.BaseStream.Position;
                    recordFiles(collectFonts(input_folder), "FONT");
                    bread.BaseStream.Position = bacp;
                    bwrite.Close();
                    filesToCreate.Clear();
                }
                else if (chunk_name == "TXTR")
                {
                    List<uint> entries = collect_entries(false);
                    for (int i = 0; i < entries.Count - 1; i++)
                    {
                        uint offset = entries[i];
                        bread.BaseStream.Position = offset + 4;
                        offset = bread.ReadUInt32();
                        entries[i] = offset;
                    }
                    filesToCreate = new List<endFiles>();
                    for (int i = 0; i < entries.Count - 1; i++)
                    {
                        uint offset = entries[i];
                        uint next_offset = entries[i + 1];
                        uint size = next_offset - offset;
                        endFiles f1 = new endFiles();
                        f1.name = "" + i + ".png";
                        f1.offset = offset;
                        f1.size = size;
                        filesToCreate.Add(f1);
                    }
                }
                else if (chunk_name == "AUDO")
                {
                    List<uint> entries = collect_entries(false);
                    filesToCreate = new List<endFiles>();
                    for (int i = 0; i < entries.Count - 1; i++)
                    {
                        uint offset = entries[i];
                        bread.BaseStream.Position = offset;
                        uint size = bread.ReadUInt32();
                        offset = (uint)bread.BaseStream.Position;
                        endFiles f1 = new endFiles();
                        f1.name = "" + i + ".wav";
                        f1.offset = offset;
                        f1.size = size;
                        filesToCreate.Add(f1);
                    }
                }
                else if (chunk_name == "FONT")
                {
                    FONT_offset = (uint)bread.BaseStream.Position;
                    FONT_limit = chunk_limit;
                }

                if (chunk_name != "FORM")
                    if (filesToCreate.Count == 0)
                    {
                        string name = "CHUNK//" + chunk_name + ".chunk";
                        uint bu = (uint)bread.BaseStream.Position;
                        bread.BaseStream.Position = chunk_offset;

                        bwrite = new BinaryWriter(File.Open(input_folder + name, FileMode.Create));
                        for (uint i = 0; i < chunk_size; i++)
                            bwrite.Write(bread.ReadByte());
                        bread.BaseStream.Position = bu;
                        bwrite.Close();
                    }
                    else
                    {
                        recordFiles(filesToCreate, chunk_name);

                        if (chunk_name == "TXTR") collectFontImages();
                    }

                chunk_offset += chunk_size;
                bread.BaseStream.Position = chunk_offset;
            }

            var translateFile = File.Open(input_folder + "translate.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            translateFile.Close();
            Directory.CreateDirectory(input_folder + "patch");
            var patchFile = File.Open(input_folder + "patch\\patch.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            bread.Close();
            patchFile.Close();
        }

        static List<uint> collect_entries(bool fnt)
        {
            List<uint> entries = new List<uint>();
            uint files = bread.ReadUInt32();
            for (uint i = 0; i < files; i++)
            {
                uint offset = bread.ReadUInt32();
                if (offset != 0)
                {
                    entries.Add(offset);
                }
            }
            entries.Add(fnt ? FONT_limit : chunk_limit);
            return entries;
        }

        static void recordFiles(List<endFiles> files, string folder)
        {
            Directory.CreateDirectory(input_folder + folder);
            for (int i = 0; i < files.Count; i++)
            {
                string name = files[i].name;
                uint bu = (uint)bread.BaseStream.Position;
                bread.BaseStream.Position = files[i].offset;

                bwrite = new BinaryWriter(File.Open(input_folder + folder + "\\" + name, FileMode.Create));
                for (uint j = 0; j < files[i].size; j++)
                    bwrite.Write(bread.ReadByte());
                bwrite.Close();
                bread.BaseStream.Position = bu;
            }
        }

        static List<endFiles> collectFonts(string input_folder)
        {
            bread.BaseStream.Position = FONT_offset;
            List<uint> entries = collect_entries(true);
            List<endFiles> filesToCreate = new List<endFiles>();
            for (int i = 0; i < entries.Count - 1; i++)
            {
                uint offset = entries[i];
                bread.BaseStream.Position = offset;
                endFiles f1 = new endFiles();
                string font_name = getSTRGEntry(bread.ReadUInt32());
                string font_family = getSTRGEntry(bread.ReadUInt32());
                f1.name = "" + i + "_" + font_name + " (" + font_family + ")";
                f1.offset = offset;
                f1.size = calculateFontSize(offset);
                filesToCreate.Add(f1);
            }
            return filesToCreate;
        }

        static uint calculateFontSize(uint font_offset)
        {
            uint result = 44;
            long bacup = bread.BaseStream.Position;

            bread.BaseStream.Position = font_offset + 40;
            uint glyphs = bread.ReadUInt32();
            result += glyphs * 20;

            bread.BaseStream.Position = bacup;
            return result;
        }

        static void collectFontImages()
        {
            long bacup = bread.BaseStream.Position;
            bread.BaseStream.Position = FONT_offset;
            List<uint> fonts = collect_entries(false);
            for (int f = 0; f < fonts.Count - 1; f++)
            {
                bread.BaseStream.Position = fonts[f] + 28;
                spriteInfo sprt = getSpriteInfo(bread.ReadUInt32());
                Bitmap texture = new Bitmap(Image.FromFile(input_folder + "TXTR\\" + sprt.i + ".png"));
                Bitmap cropped = texture.Clone(new Rectangle((int)sprt.x, (int)sprt.y, (int)sprt.w, (int)sprt.h), texture.PixelFormat);
                cropped.Save(input_folder + "FONT\\" + f + ".png");
            }

            bread.BaseStream.Position = bacup;
        }

        static spriteInfo getSpriteInfo(uint sprite_offset)
        {
            spriteInfo result = new spriteInfo();
            long bacup = bread.BaseStream.Position;
            bread.BaseStream.Position = sprite_offset;
            result.x = bread.ReadUInt16();
            result.y = bread.ReadUInt16();
            result.w = bread.ReadUInt16();
            result.h = bread.ReadUInt16();
            bread.BaseStream.Position += 12;
            result.i = bread.ReadUInt16();
            bread.BaseStream.Position = bacup;
            return result;
        }

        static string getSTRGEntry(uint str_offset)
        {
            long bacup = bread.BaseStream.Position;
            bread.BaseStream.Position = str_offset - 4;//???
            byte[] strar = new byte[bread.ReadInt32()];
            for (int f = 0; f < strar.Length; f++)
                strar[f] = bread.ReadByte();

            bread.BaseStream.Position = bacup;
            return System.Text.Encoding.ASCII.GetString(strar);//UTF-8?
        }
    }
}
