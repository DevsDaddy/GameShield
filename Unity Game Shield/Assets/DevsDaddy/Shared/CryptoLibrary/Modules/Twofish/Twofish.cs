using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.Twofish
{
    public sealed class Twofish : SymmetricAlgorithm
    {
        public Twofish() : base() {
            KeySizeValue = KeySizeInBits;
            BlockSizeValue = BlockSizeInBits;
            FeedbackSizeValue = BlockSizeValue;
            LegalBlockSizesValue = new KeySizes[] { new KeySizes(BlockSizeInBits, BlockSizeInBits, 0) };
            LegalKeySizesValue = new KeySizes[] { new KeySizes(128, 256, 64) };  // 128, 192, or 256

            base.Mode = CipherMode.CBC;  // same as default
            base.Padding = PaddingMode.PKCS7;
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? rgbIV) {
            if (rgbKey == null) { throw new ArgumentNullException(nameof(rgbKey), "Key cannot be null."); }
            if (rgbKey.Length != KeySize / 8) { throw new ArgumentOutOfRangeException(nameof(rgbKey), "Key size mismatch."); }
            if (Mode == CipherMode.CBC) {
                if (rgbIV == null) { throw new ArgumentNullException(nameof(rgbIV), "IV cannot be null."); }
                if (rgbIV.Length != 16) { throw new ArgumentOutOfRangeException(nameof(rgbIV), "Invalid IV size."); }
            }

            return new TwofishTransform(rgbKey, rgbIV, TwofishTransformMode.Decrypt, Mode, Padding);
        }
        
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? rgbIV) {
            if (rgbKey == null) { throw new ArgumentNullException(nameof(rgbKey), "Key cannot be null."); }
            if (rgbKey.Length != KeySize / 8) { throw new ArgumentOutOfRangeException(nameof(rgbKey), "Key size mismatch."); }
            if (Mode == CipherMode.CBC) {
                if (rgbIV == null) { throw new ArgumentNullException(nameof(rgbIV), "IV cannot be null."); }
                if (rgbIV.Length != 16) { throw new ArgumentOutOfRangeException(nameof(rgbIV), "Invalid IV size."); }
            }

            return new TwofishTransform(rgbKey, rgbIV, TwofishTransformMode.Encrypt, Mode, Padding);
        }
        
        public override void GenerateIV() {
            byte[] rndKey = new byte[FeedbackSizeValue / 8];
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            generator.GetBytes(rndKey);
            IVValue = rndKey;
        }
        
        public override void GenerateKey() {
            byte[] rndKey = new byte[KeySizeValue / 8];
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            generator.GetBytes(rndKey);
            KeyValue = rndKey;
        }
        
        public override CipherMode Mode {
            get { return base.Mode; }
            set {
                switch (value) {
                    case CipherMode.CBC: break;
#pragma warning disable CA5358 // While using ECB is not recommended, it's still supported
                    case CipherMode.ECB: break;
#pragma warning restore CA5358 // Review cipher mode usage with cryptography experts
                    default: throw new CryptographicException("Cipher mode is not supported.");
                }
                base.Mode = value;
            }
        }
        
        public override PaddingMode Padding {
            get { return base.Padding; }
            set {
                base.Padding = value switch {
                    PaddingMode.None => value,
                    PaddingMode.PKCS7 => value,
                    PaddingMode.Zeros => value,
                    PaddingMode.ANSIX923 => value,
                    PaddingMode.ISO10126 => value,
                    _ => throw new CryptographicException("Padding mode is not supported."),
                };
            }
        }
        
        private const int KeySizeInBits = 256;
        private const int BlockSizeInBits = 128;
    }
    
    enum TwofishTransformMode {
        Encrypt = 0,
        Decrypt = 1
    }

    sealed class TwofishTransform : ICryptoTransform
    {
        internal TwofishTransform(byte[] rgbKey, byte[]? rgbIV, TwofishTransformMode transformMode, CipherMode cipherMode, PaddingMode paddingMode) {
            if (rgbKey == null) { throw new ArgumentNullException(nameof(rgbKey), "Key cannot be null."); }
            if (rgbKey.Length is not 16 and not 24 and not 32) { throw new ArgumentOutOfRangeException(nameof(rgbKey), "Key must be 128, 192, or 256 bits."); }
            if ((rgbIV is not null) && (rgbIV.Length != 16)) { throw new ArgumentOutOfRangeException(nameof(rgbKey), "IV must be 128 bits."); }
            CipherMode = cipherMode;
            TransformMode = transformMode;
            PaddingMode = paddingMode;

            DecryptionBuffer = new byte[16];

            Key = new DWord[rgbKey.Length / 4];
            SBoxKeys = new DWord[MaxKeyBits / 64];
            SubKeys = new DWord[TotalSubkeys];

            var key32 = new uint[Key.Length];
            Buffer.BlockCopy(rgbKey, 0, key32, 0, rgbKey.Length);
            for (var i = 0; i < Key.Length; i++) { Key[i] = (DWord)key32[i]; }
            Array.Clear(key32, 0, key32.Length);

            if (rgbIV != null) {
                IV = new DWord[rgbIV.Length];
                var iv32 = new uint[IV.Length];
                Buffer.BlockCopy(rgbIV, 0, iv32, 0, rgbIV.Length);
                for (var i = 0; i < IV.Length; i++) { IV[i] = (DWord)iv32[i]; }
                Array.Clear(iv32, 0, iv32.Length);
            }

            ReKey();
        }
        
        private readonly TwofishTransformMode TransformMode;
        private readonly CipherMode CipherMode;
        private readonly PaddingMode PaddingMode;
        
        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        public bool CanReuseTransform { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        public bool CanTransformMultipleBlocks { get { return true; } }

        /// <summary>
        /// Gets the input block size (in bytes).
        /// </summary>
        public int InputBlockSize { get { return 16; } } //block is always 128 bits

        /// <summary>
        /// Gets the output block size (in bytes).
        /// </summary>
        public int OutputBlockSize { get { return 16; } } //block is always 128 bits

        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }
        
        private void Dispose(bool disposing) {
            if (disposing) {
                Array.Clear(Key, 0, Key.Length);
                if (IV != null) { Array.Clear(IV, 0, IV.Length); }
                Array.Clear(SBoxKeys, 0, SBoxKeys.Length);
                Array.Clear(SubKeys, 0, SubKeys.Length);
                Array.Clear(DecryptionBuffer, 0, DecryptionBuffer.Length);
            }
        }
        
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
            if (inputBuffer == null) { throw new ArgumentNullException(nameof(inputBuffer), "Input buffer cannot be null."); }
            if (outputBuffer == null) { throw new ArgumentNullException(nameof(outputBuffer), "Output buffer cannot be null."); }
            if (inputOffset < 0) { throw new ArgumentOutOfRangeException(nameof(inputOffset), "Input offset must be non-negative number."); }
            if (outputOffset < 0) { throw new ArgumentOutOfRangeException(nameof(outputOffset), "Output offset must be non-negative number."); }
            if ((inputCount <= 0) || (inputCount % 16 != 0) || (inputCount > inputBuffer.Length)) { throw new ArgumentOutOfRangeException(nameof(inputCount), "Invalid input count."); }
            if ((inputBuffer.Length - inputCount) < inputOffset) { throw new ArgumentOutOfRangeException(nameof(inputCount), "Invalid input length."); }
            if (outputOffset + inputCount > outputBuffer.Length) { throw new ArgumentOutOfRangeException(nameof(outputOffset), "Insufficient output buffer."); }

            if (TransformMode == TwofishTransformMode.Encrypt) {

                for (var i = 0; i < inputCount; i += 16) {
                    BlockEncrypt(inputBuffer, inputOffset + i, outputBuffer, outputOffset + i);
                }
                return inputCount;

            } else {  // Decrypt

                var bytesWritten = 0;

                if (DecryptionBufferInUse) {  // process the last block of previous round
                    BlockDecrypt(DecryptionBuffer, 0, outputBuffer, outputOffset);
                    DecryptionBufferInUse = false;
                    outputOffset += 16;
                    bytesWritten += 16;
                }

                for (var i = 0; i < inputCount - 16; i += 16) {
                    BlockDecrypt(inputBuffer, inputOffset + i, outputBuffer, outputOffset);
                    outputOffset += 16;
                    bytesWritten += 16;
                }

                if (PaddingMode == PaddingMode.None) {
                    BlockDecrypt(inputBuffer, inputOffset + inputCount - 16, outputBuffer, outputOffset);
                    return inputCount;
                } else {  // save last block without processing because decryption otherwise cannot detect padding in CryptoStream
                    Buffer.BlockCopy(inputBuffer, inputOffset + inputCount - 16, DecryptionBuffer, 0, 16);
                    DecryptionBufferInUse = true;
                }

                return bytesWritten;

            }
        }
        
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
            if (inputBuffer == null) { throw new ArgumentNullException(nameof(inputBuffer), "Input buffer cannot be null."); }
            if (inputOffset < 0) { throw new ArgumentOutOfRangeException(nameof(inputOffset), "Input offset must be non-negative number."); }
            if ((inputCount < 0) || (inputCount > inputBuffer.Length)) { throw new ArgumentOutOfRangeException(nameof(inputCount), "Invalid input count."); }
            if ((PaddingMode == PaddingMode.None) && (inputCount % 16 != 0)) { throw new ArgumentOutOfRangeException(nameof(inputCount), "No padding for final block."); }
            if ((inputBuffer.Length - inputCount) < inputOffset) { throw new ArgumentOutOfRangeException(nameof(inputCount), "Invalid input length."); }

            if (TransformMode == TwofishTransformMode.Encrypt) {

                int paddedLength;
                byte[] paddedInputBuffer;
                int paddedInputOffset;
                switch (PaddingMode) {
                    case PaddingMode.None:
                        paddedLength = inputCount;
                        paddedInputBuffer = inputBuffer;
                        paddedInputOffset = inputOffset;
                        break;

                    case PaddingMode.PKCS7:
                        paddedLength = inputCount / 16 * 16 + 16; //to round to next whole block
                        paddedInputBuffer = new byte[paddedLength];
                        paddedInputOffset = 0;
                        Buffer.BlockCopy(inputBuffer, inputOffset, paddedInputBuffer, 0, inputCount);
                        var added = (byte)(paddedLength - inputCount);
                        for (var i = inputCount; i < inputCount + added; i++) {
                            paddedInputBuffer[i] = added;
                        }
                        break;

                    case PaddingMode.Zeros:
                        paddedLength = (inputCount + 15) / 16 * 16; //to round to next whole block
                        paddedInputBuffer = new byte[paddedLength];
                        paddedInputOffset = 0;
                        Buffer.BlockCopy(inputBuffer, inputOffset, paddedInputBuffer, 0, inputCount);
                        break;

                    case PaddingMode.ANSIX923:
                        paddedLength = inputCount / 16 * 16 + 16; //to round to next whole block
                        paddedInputBuffer = new byte[paddedLength];
                        paddedInputOffset = 0;
                        Buffer.BlockCopy(inputBuffer, inputOffset, paddedInputBuffer, 0, inputCount);
                        paddedInputBuffer[^1] = (byte)(paddedLength - inputCount);
                        break;

                    case PaddingMode.ISO10126:
                        paddedLength = inputCount / 16 * 16 + 16; //to round to next whole block
                        paddedInputBuffer = new byte[paddedLength];
                        RandomNumberGenerator.Fill(paddedInputBuffer.AsSpan(inputCount));
                        paddedInputOffset = 0;
                        Buffer.BlockCopy(inputBuffer, inputOffset, paddedInputBuffer, 0, inputCount);
                        paddedInputBuffer[^1] = (byte)(paddedLength - inputCount);
                        break;

                    default: throw new CryptographicException("Unsupported padding mode.");
                }

                var outputBuffer = new byte[paddedLength];

                for (var i = 0; i < paddedLength; i += 16) {
                    BlockEncrypt(paddedInputBuffer, paddedInputOffset + i, outputBuffer, i);
                }

                return outputBuffer;

            } else {  // Decrypt

                byte[] outputBuffer;

                if (PaddingMode == PaddingMode.None) {
                    outputBuffer = new byte[inputCount];
                } else if (inputCount % 16 != 0) {
                    throw new ArgumentOutOfRangeException(nameof(inputCount), "Invalid input count.");
                } else {
                    outputBuffer = new byte[inputCount + (DecryptionBufferInUse ? 16 : 0)];
                }

                var outputOffset = 0;

                if (DecryptionBufferInUse) {  // process leftover padding buffer to keep CryptoStream happy
                    BlockDecrypt(DecryptionBuffer, 0, outputBuffer, 0);
                    DecryptionBufferInUse = false;
                    outputOffset = 16;
                }

                for (var i = 0; i < inputCount; i += 16) {
                    BlockDecrypt(inputBuffer, inputOffset + i, outputBuffer, outputOffset + i);
                }

                return RemovePadding(outputBuffer, PaddingMode);

            }
        }
        
        private readonly byte[] DecryptionBuffer;
        private bool DecryptionBufferInUse;
        
        private static byte[] RemovePadding(byte[] outputBuffer, PaddingMode paddingMode) {
            if (paddingMode == PaddingMode.PKCS7) {
                var padding = outputBuffer[^1];
                if (padding is < 1 or > 16) { throw new CryptographicException("Invalid padding."); }
                for (var i = outputBuffer.Length - padding; i < outputBuffer.Length; i++) {
                    if (outputBuffer[i] != padding) { throw new CryptographicException("Invalid padding."); }
                }
                var newOutputBuffer = new byte[outputBuffer.Length - padding];
                Buffer.BlockCopy(outputBuffer, 0, newOutputBuffer, 0, newOutputBuffer.Length);
                return newOutputBuffer;
            } else if (paddingMode == PaddingMode.Zeros) {
                var newOutputLength = outputBuffer.Length;
                for (var i = outputBuffer.Length - 1; i >= Math.Max(outputBuffer.Length - 16, 0); i--) {
                    if (outputBuffer[i] != 0) {
                        newOutputLength = i + 1;
                        break;
                    }
                }
                if (newOutputLength == outputBuffer.Length) {
                    return outputBuffer;
                } else {
                    var newOutputBuffer = new byte[newOutputLength];
                    Buffer.BlockCopy(outputBuffer, 0, newOutputBuffer, 0, newOutputBuffer.Length);
                    return newOutputBuffer;
                }
            } else if (paddingMode == PaddingMode.ANSIX923) {
                var padding = outputBuffer[^1];
                if (padding is < 1 or > 16) { throw new CryptographicException("Invalid padding."); }
                for (var i = outputBuffer.Length - padding; i < outputBuffer.Length - 1; i++) {
                    if (outputBuffer[i] != 0) { throw new CryptographicException("Invalid padding."); }
                }
                var newOutputBuffer = new byte[outputBuffer.Length - padding];
                Buffer.BlockCopy(outputBuffer, 0, newOutputBuffer, 0, newOutputBuffer.Length);
                return newOutputBuffer;
            } else if (paddingMode == PaddingMode.ISO10126) {
                var padding = outputBuffer[^1];
                if (padding is < 1 or > 16) { throw new CryptographicException("Invalid padding."); }
                var newOutputBuffer = new byte[outputBuffer.Length - padding];
                Buffer.BlockCopy(outputBuffer, 0, newOutputBuffer, 0, newOutputBuffer.Length);
                return newOutputBuffer;
            } else {  // None
                return outputBuffer;
            }
        }
        
        private const int BlockSize = 128; //number of bits per block
        private const int Rounds = 16; //default number of rounds for 128/192/256-bit keys
        private const int MaxKeyBits = 256; //max number of bits of key

        private const int InputWhiten = 0;
        private const int OutputWhiten = (InputWhiten + BlockSize / 32);
        private const int RoundSubkeys = (OutputWhiten + BlockSize / 32);
        private const int TotalSubkeys = (RoundSubkeys + 2 * Rounds);

        private readonly DWord[] Key;
        private readonly DWord[]? IV;
        private readonly DWord[] SBoxKeys;
        private readonly DWord[] SubKeys;
        
        private const int SubkeyStep = 0x02020202;
        private const int SubkeyBump = 0x01010101;
        private const int SubkeyRotateLeft = 9;
        
        private void ReKey() {
            BuildMds(); //built only first time it is accessed

            var k32e = new DWord[Key.Length / 2];
            var k32o = new DWord[Key.Length / 2]; //even/odd key dwords

            var k64Cnt = Key.Length / 2;
            for (var i = 0; i < k64Cnt; i++) { //split into even/odd key dwords
                k32e[i] = Key[2 * i];
                k32o[i] = Key[2 * i + 1];
                SBoxKeys[k64Cnt - 1 - i] = ReedSolomonMdsEncode(k32e[i], k32o[i]); //compute S-box keys using (12,8) Reed-Solomon code over GF(256)
            }

            var subkeyCnt = RoundSubkeys + 2 * Rounds;
            var keyLen = Key.Length * 4 * 8;
            for (var i = 0; i < subkeyCnt / 2; i++) { //compute round subkeys for PHT
                var A = F32((DWord)(i * SubkeyStep), k32e, keyLen); //A uses even key dwords
                var B = F32((DWord)(i * SubkeyStep + SubkeyBump), k32o, keyLen);   //B uses odd  key dwords
                B = B.RotateLeft(8);
                SubKeys[2 * i] = A + B; //combine with a PHT
                SubKeys[2 * i + 1] = (A + 2 * B).RotateLeft(SubkeyRotateLeft);
            }
        }
        
        internal void BlockEncrypt(byte[] inputBuffer, int inputOffset, byte[] outputBuffer, int outputBufferOffset) {
            var x = new DWord[BlockSize / 32];
            for (var i = 0; i < BlockSize / 32; i++) { //copy in the block, add whitening
                x[i] = new DWord(inputBuffer, inputOffset + i * 4) ^ SubKeys[InputWhiten + i];
                if ((CipherMode == CipherMode.CBC) && (IV != null)) { x[i] ^= IV[i]; }
            }

            var keyLen = Key.Length * 4 * 8;
            for (var r = 0; r < Rounds; r++) { //main Twofish encryption loop
                var t0 = F32(x[0], SBoxKeys, keyLen);
                var t1 = F32(x[1].RotateLeft(8), SBoxKeys, keyLen);

                x[3] = x[3].RotateLeft(1);
                x[2] ^= t0 + t1 + SubKeys[RoundSubkeys + 2 * r]; //PHT, round keys
                x[3] ^= t0 + 2 * t1 + SubKeys[RoundSubkeys + 2 * r + 1];
                x[2] = x[2].RotateRight(1);

                if (r < Rounds - 1) { //swap for next round
                    (x[2], x[0]) = (x[0], x[2]);
                    (x[3], x[1]) = (x[1], x[3]);
                }
            }

            for (var i = 0; i < BlockSize / 32; i++) { //copy out, with whitening
                var outValue = x[i] ^ SubKeys[OutputWhiten + i];
                outputBuffer[outputBufferOffset + i * 4 + 0] = outValue.B0;
                outputBuffer[outputBufferOffset + i * 4 + 1] = outValue.B1;
                outputBuffer[outputBufferOffset + i * 4 + 2] = outValue.B2;
                outputBuffer[outputBufferOffset + i * 4 + 3] = outValue.B3;
                if ((CipherMode == CipherMode.CBC) && (IV != null)) { IV[i] = outValue; }
            }
        }
        
        internal void BlockDecrypt(byte[] inputBuffer, int inputOffset, byte[] outputBuffer, int outputBufferOffset) {
            var x = new DWord[BlockSize / 32];
            var input = new DWord[BlockSize / 32];
            for (var i = 0; i < BlockSize / 32; i++) { //copy in the block, add whitening
                input[i] = new DWord(inputBuffer, inputOffset + i * 4);
                x[i] = input[i] ^ SubKeys[OutputWhiten + i];
            }

            var keyLen = Key.Length * 4 * 8;
            for (var r = Rounds - 1; r >= 0; r--) { //main Twofish decryption loop
                var t0 = F32(x[0], SBoxKeys, keyLen);
                var t1 = F32(x[1].RotateLeft(8), SBoxKeys, keyLen);

                x[2] = x[2].RotateLeft(1);
                x[2] ^= t0 + t1 + SubKeys[RoundSubkeys + 2 * r]; //PHT, round keys
                x[3] ^= t0 + 2 * t1 + SubKeys[RoundSubkeys + 2 * r + 1];
                x[3] = x[3].RotateRight(1);

                if (r > 0) { //unswap, except for last round
                    t0 = x[0]; x[0] = x[2]; x[2] = t0;
                    t1 = x[1]; x[1] = x[3]; x[3] = t1;
                }
            }

            for (var i = 0; i < BlockSize / 32; i++) { //copy out, with whitening
                x[i] ^= SubKeys[InputWhiten + i];
                if ((CipherMode == CipherMode.CBC) && (IV != null)) {
                    x[i] ^= IV[i];
                    IV[i] = input[i];
                }
                outputBuffer[outputBufferOffset + i * 4 + 0] = x[i].B0;
                outputBuffer[outputBufferOffset + i * 4 + 1] = x[i].B1;
                outputBuffer[outputBufferOffset + i * 4 + 2] = x[i].B2;
                outputBuffer[outputBufferOffset + i * 4 + 3] = x[i].B3;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DWord F32(DWord x, DWord[] k32, int keyLen) {
            if (keyLen >= 256) {
                x.B0 = (byte)(P8x8[P_04][x.B0] ^ k32[3].B0);
                x.B1 = (byte)(P8x8[P_14][x.B1] ^ k32[3].B1);
                x.B2 = (byte)(P8x8[P_24][x.B2] ^ k32[3].B2);
                x.B3 = (byte)(P8x8[P_34][x.B3] ^ k32[3].B3);
            }
            if (keyLen >= 192) {
                x.B0 = (byte)(P8x8[P_03][x.B0] ^ k32[2].B0);
                x.B1 = (byte)(P8x8[P_13][x.B1] ^ k32[2].B1);
                x.B2 = (byte)(P8x8[P_23][x.B2] ^ k32[2].B2);
                x.B3 = (byte)(P8x8[P_33][x.B3] ^ k32[2].B3);
            }
            if (keyLen >= 128) {
                x = MdsTable[0][P8x8[P_01][P8x8[P_02][x.B0] ^ k32[1].B0] ^ k32[0].B0]
                    ^ MdsTable[1][P8x8[P_11][P8x8[P_12][x.B1] ^ k32[1].B1] ^ k32[0].B1]
                    ^ MdsTable[2][P8x8[P_21][P8x8[P_22][x.B2] ^ k32[1].B2] ^ k32[0].B2]
                    ^ MdsTable[3][P8x8[P_31][P8x8[P_32][x.B3] ^ k32[1].B3] ^ k32[0].B3];
            }

            return x;
        }
        
        private const uint P_01 = 0;
        private const uint P_02 = 0;
        private const uint P_03 = (P_01 ^ 1);  // "extend" to larger key sizes
        private const uint P_04 = 1;

        private const uint P_11 = 0;
        private const uint P_12 = 1;
        private const uint P_13 = (P_11 ^ 1);
        private const uint P_14 = 0;

        private const uint P_21 = 1;
        private const uint P_22 = 0;
        private const uint P_23 = (P_21 ^ 1);
        private const uint P_24 = 0;

        private const uint P_31 = 1;
        private const uint P_32 = 1;
        private const uint P_33 = (P_31 ^ 1);
        private const uint P_34 = 1;
        
        private static readonly byte[][] P8x8 = new byte[][] {
                                            new byte[] {
                                                0xA9, 0x67, 0xB3, 0xE8, 0x04, 0xFD, 0xA3, 0x76,
                                                0x9A, 0x92, 0x80, 0x78, 0xE4, 0xDD, 0xD1, 0x38,
                                                0x0D, 0xC6, 0x35, 0x98, 0x18, 0xF7, 0xEC, 0x6C,
                                                0x43, 0x75, 0x37, 0x26, 0xFA, 0x13, 0x94, 0x48,
                                                0xF2, 0xD0, 0x8B, 0x30, 0x84, 0x54, 0xDF, 0x23,
                                                0x19, 0x5B, 0x3D, 0x59, 0xF3, 0xAE, 0xA2, 0x82,
                                                0x63, 0x01, 0x83, 0x2E, 0xD9, 0x51, 0x9B, 0x7C,
                                                0xA6, 0xEB, 0xA5, 0xBE, 0x16, 0x0C, 0xE3, 0x61,
                                                0xC0, 0x8C, 0x3A, 0xF5, 0x73, 0x2C, 0x25, 0x0B,
                                                0xBB, 0x4E, 0x89, 0x6B, 0x53, 0x6A, 0xB4, 0xF1,
                                                0xE1, 0xE6, 0xBD, 0x45, 0xE2, 0xF4, 0xB6, 0x66,
                                                0xCC, 0x95, 0x03, 0x56, 0xD4, 0x1C, 0x1E, 0xD7,
                                                0xFB, 0xC3, 0x8E, 0xB5, 0xE9, 0xCF, 0xBF, 0xBA,
                                                0xEA, 0x77, 0x39, 0xAF, 0x33, 0xC9, 0x62, 0x71,
                                                0x81, 0x79, 0x09, 0xAD, 0x24, 0xCD, 0xF9, 0xD8,
                                                0xE5, 0xC5, 0xB9, 0x4D, 0x44, 0x08, 0x86, 0xE7,
                                                0xA1, 0x1D, 0xAA, 0xED, 0x06, 0x70, 0xB2, 0xD2,
                                                0x41, 0x7B, 0xA0, 0x11, 0x31, 0xC2, 0x27, 0x90,
                                                0x20, 0xF6, 0x60, 0xFF, 0x96, 0x5C, 0xB1, 0xAB,
                                                0x9E, 0x9C, 0x52, 0x1B, 0x5F, 0x93, 0x0A, 0xEF,
                                                0x91, 0x85, 0x49, 0xEE, 0x2D, 0x4F, 0x8F, 0x3B,
                                                0x47, 0x87, 0x6D, 0x46, 0xD6, 0x3E, 0x69, 0x64,
                                                0x2A, 0xCE, 0xCB, 0x2F, 0xFC, 0x97, 0x05, 0x7A,
                                                0xAC, 0x7F, 0xD5, 0x1A, 0x4B, 0x0E, 0xA7, 0x5A,
                                                0x28, 0x14, 0x3F, 0x29, 0x88, 0x3C, 0x4C, 0x02,
                                                0xB8, 0xDA, 0xB0, 0x17, 0x55, 0x1F, 0x8A, 0x7D,
                                                0x57, 0xC7, 0x8D, 0x74, 0xB7, 0xC4, 0x9F, 0x72,
                                                0x7E, 0x15, 0x22, 0x12, 0x58, 0x07, 0x99, 0x34,
                                                0x6E, 0x50, 0xDE, 0x68, 0x65, 0xBC, 0xDB, 0xF8,
                                                0xC8, 0xA8, 0x2B, 0x40, 0xDC, 0xFE, 0x32, 0xA4,
                                                0xCA, 0x10, 0x21, 0xF0, 0xD3, 0x5D, 0x0F, 0x00,
                                                0x6F, 0x9D, 0x36, 0x42, 0x4A, 0x5E, 0xC1, 0xE0
                                            },
                                            new byte[] {
                                                0x75, 0xF3, 0xC6, 0xF4, 0xDB, 0x7B, 0xFB, 0xC8,
                                                0x4A, 0xD3, 0xE6, 0x6B, 0x45, 0x7D, 0xE8, 0x4B,
                                                0xD6, 0x32, 0xD8, 0xFD, 0x37, 0x71, 0xF1, 0xE1,
                                                0x30, 0x0F, 0xF8, 0x1B, 0x87, 0xFA, 0x06, 0x3F,
                                                0x5E, 0xBA, 0xAE, 0x5B, 0x8A, 0x00, 0xBC, 0x9D,
                                                0x6D, 0xC1, 0xB1, 0x0E, 0x80, 0x5D, 0xD2, 0xD5,
                                                0xA0, 0x84, 0x07, 0x14, 0xB5, 0x90, 0x2C, 0xA3,
                                                0xB2, 0x73, 0x4C, 0x54, 0x92, 0x74, 0x36, 0x51,
                                                0x38, 0xB0, 0xBD, 0x5A, 0xFC, 0x60, 0x62, 0x96,
                                                0x6C, 0x42, 0xF7, 0x10, 0x7C, 0x28, 0x27, 0x8C,
                                                0x13, 0x95, 0x9C, 0xC7, 0x24, 0x46, 0x3B, 0x70,
                                                0xCA, 0xE3, 0x85, 0xCB, 0x11, 0xD0, 0x93, 0xB8,
                                                0xA6, 0x83, 0x20, 0xFF, 0x9F, 0x77, 0xC3, 0xCC,
                                                0x03, 0x6F, 0x08, 0xBF, 0x40, 0xE7, 0x2B, 0xE2,
                                                0x79, 0x0C, 0xAA, 0x82, 0x41, 0x3A, 0xEA, 0xB9,
                                                0xE4, 0x9A, 0xA4, 0x97, 0x7E, 0xDA, 0x7A, 0x17,
                                                0x66, 0x94, 0xA1, 0x1D, 0x3D, 0xF0, 0xDE, 0xB3,
                                                0x0B, 0x72, 0xA7, 0x1C, 0xEF, 0xD1, 0x53, 0x3E,
                                                0x8F, 0x33, 0x26, 0x5F, 0xEC, 0x76, 0x2A, 0x49,
                                                0x81, 0x88, 0xEE, 0x21, 0xC4, 0x1A, 0xEB, 0xD9,
                                                0xC5, 0x39, 0x99, 0xCD, 0xAD, 0x31, 0x8B, 0x01,
                                                0x18, 0x23, 0xDD, 0x1F, 0x4E, 0x2D, 0xF9, 0x48,
                                                0x4F, 0xF2, 0x65, 0x8E, 0x78, 0x5C, 0x58, 0x19,
                                                0x8D, 0xE5, 0x98, 0x57, 0x67, 0x7F, 0x05, 0x64,
                                                0xAF, 0x63, 0xB6, 0xFE, 0xF5, 0xB7, 0x3C, 0xA5,
                                                0xCE, 0xE9, 0x68, 0x44, 0xE0, 0x4D, 0x43, 0x69,
                                                0x29, 0x2E, 0xAC, 0x15, 0x59, 0xA8, 0x0A, 0x9E,
                                                0x6E, 0x47, 0xDF, 0x34, 0x35, 0x6A, 0xCF, 0xDC,
                                                0x22, 0xC9, 0xC0, 0x9B, 0x89, 0xD4, 0xED, 0xAB,
                                                0x12, 0xA2, 0x0D, 0x52, 0xBB, 0x02, 0x2F, 0xA9,
                                                0xD7, 0x61, 0x1E, 0xB4, 0x50, 0x04, 0xF6, 0xC2,
                                                0x16, 0x25, 0x86, 0x56, 0x55, 0x09, 0xBE, 0x91
                                            }
                                          };
        
        private static readonly DWord[][] MdsTable = new DWord[4][] { new DWord[256], new DWord[256], new DWord[256], new DWord[256] };
        private static bool MdsTableBuilt;
        private static readonly object SyncRootBuildMds = new();
        
        private static void BuildMds() {
            lock (SyncRootBuildMds) {
                if (MdsTableBuilt) { return; }

                var m1 = new byte[2];
                var mX = new byte[2];
                var mY = new byte[4];

                for (var i = 0; i < 256; i++) {
                    m1[0] = P8x8[0][i];     /* compute all the matrix elements */
                    mX[0] = (byte)Mul_X(m1[0]);
                    mY[0] = (byte)Mul_Y(m1[0]);

                    m1[1] = P8x8[1][i];
                    mX[1] = (byte)Mul_X(m1[1]);
                    mY[1] = (byte)Mul_Y(m1[1]);

                    MdsTable[0][i].B0 = m1[1];
                    MdsTable[0][i].B1 = mX[1];
                    MdsTable[0][i].B2 = mY[1];
                    MdsTable[0][i].B3 = mY[1]; //SetMDS(0);

                    MdsTable[1][i].B0 = mY[0];
                    MdsTable[1][i].B1 = mY[0];
                    MdsTable[1][i].B2 = mX[0];
                    MdsTable[1][i].B3 = m1[0]; //SetMDS(1);

                    MdsTable[2][i].B0 = mX[1];
                    MdsTable[2][i].B1 = mY[1];
                    MdsTable[2][i].B2 = m1[1];
                    MdsTable[2][i].B3 = mY[1]; //SetMDS(2);

                    MdsTable[3][i].B0 = mX[0];
                    MdsTable[3][i].B1 = m1[0];
                    MdsTable[3][i].B2 = mY[0];
                    MdsTable[3][i].B3 = mX[0]; //SetMDS(3);
                }

                MdsTableBuilt = true;
            }
        }
        
        private const uint RS_GF_FDBK = 0x14D;
        
        private static DWord ReedSolomonMdsEncode(DWord k0, DWord k1) {
            var r = new DWord();
            for (var i = 0; i < 2; i++) {
                r ^= (i > 0) ? k0 : k1; //merge in 32 more key bits
                for (var j = 0; j < 4; j++) { //shift one byte at a time 
                    var b = (byte)(r >> 24);
                    var g2 = (byte)((b << 1) ^ (((b & 0x80) > 0) ? RS_GF_FDBK : 0));
                    var g3 = (byte)(((b >> 1) & 0x7F) ^ (((b & 1) > 0) ? RS_GF_FDBK >> 1 : 0) ^ g2);
                    r.B3 = (byte)(r.B2 ^ g3);
                    r.B2 = (byte)(r.B1 ^ g2);
                    r.B1 = (byte)(r.B0 ^ g3);
                    r.B0 = b;
                }
            }
            return r;
        }


        private static uint Mul_X(uint x) {
            return Mx_X(x);
        }
        private static uint Mul_Y(uint x) {
            return Mx_Y(x);
        }
        
        private static uint Mx_X(uint x) {
            return (x ^ LFSR2(x)); //5B
        }

        private static uint Mx_Y(uint x) {
            return (x ^ LFSR1(x) ^ LFSR2(x)); //EF
        }


        private const uint MDS_GF_FDBK = 0x169; //primitive polynomial for GF(256)

        private static uint LFSR1(uint x) {
            return ((x >> 1) ^ (((x & 0x01) > 0) ? MDS_GF_FDBK / 2 : 0));
        }

        static private uint LFSR2(uint x) {
            return ((x >> 2) ^ (((x & 0x02) > 0) ? MDS_GF_FDBK / 2 : 0)
                             ^ (((x & 0x01) > 0) ? MDS_GF_FDBK / 4 : 0));
        }

        private struct DWord
        {
            public byte B0;
            public byte B1;
            public byte B2;
            public byte B3;
            
            private uint Value;

            private DWord(uint value) : this() {
                Value = value;
            }

            internal DWord(byte[] buffer, int offset) : this() {
                B0 = buffer[offset + 0];
                B1 = buffer[offset + 1];
                B2 = buffer[offset + 2];
                B3 = buffer[offset + 3];
            }

            public static explicit operator uint(DWord expr) {
                return expr.Value;
            }
            
            public static explicit operator DWord(int value) {
                return new DWord((uint)value);
            }

            public static explicit operator DWord(uint value) {
                return new DWord(value);
            }


            public static DWord operator +(DWord expr1, DWord expr2) {
                expr1.Value += expr2.Value;
                return expr1;
            }

            public static DWord operator *(uint value, DWord expr) {
                expr.Value = value * expr.Value;
                return expr;
            }


            public static DWord operator |(DWord expr1, DWord expr2) {
                expr1.Value |= expr2.Value;
                return expr1;
            }

            public static DWord operator ^(DWord expr1, DWord expr2) {
                expr1.Value ^= expr2.Value;
                return expr1;
            }

            public static DWord operator <<(DWord expr, int count) {
                expr.Value <<= count;
                return expr;
            }

            public static DWord operator >>(DWord expr, int count) {
                expr.Value >>= count;
                return expr;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DWord RotateLeft(int n) {
                return (DWord)((Value << n) | (Value >> (32 - n)));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DWord RotateRight(int n) {
                return (DWord)((Value >> n) | (Value << (32 - n)));
            }
        }
    }
}