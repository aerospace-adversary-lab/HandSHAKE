
/*
 Code adapted from http://zarb.org/~gc/html/libpng.html
 */

#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <stdarg.h>
#include "fpe.h"

#define PNG_DEBUG 3
#include <png.h>

#define NUM_BLOCKS 10

void abort_(const char *s, ...)
{
  va_list args;
  va_start(args, s);
  vfprintf(stderr, s, args);
  fprintf(stderr, "\n");
  va_end(args);
  abort();
}

int x, y;

int width, height;
png_byte color_type;
png_byte bit_depth;

png_structp png_ptr;
png_infop info_ptr;
int number_of_passes;
png_bytep *row_pointers;

void read_png_file(char *file_name)
{
  char header[8]; // 8 is the maximum size that can be checked

  /* open file and test for it being a png */
  FILE *fp = fopen(file_name, "rb");
  if (!fp)
    abort_("[read_png_file] File %s could not be opened for reading", file_name);
  fread(header, 1, 8, fp);
  if (png_sig_cmp((unsigned char *)header, 0, 8))
    abort_("[read_png_file] File %s is not recognized as a PNG file", file_name);

  /* initialize stuff */
  png_ptr = png_create_read_struct(PNG_LIBPNG_VER_STRING, NULL, NULL, NULL);

  if (!png_ptr)
    abort_("[read_png_file] png_create_read_struct failed");

  info_ptr = png_create_info_struct(png_ptr);
  if (!info_ptr)
    abort_("[read_png_file] png_create_info_struct failed");

  if (setjmp(png_jmpbuf(png_ptr)))
    abort_("[read_png_file] Error during init_io");

  png_init_io(png_ptr, fp);
  png_set_sig_bytes(png_ptr, 8);

  png_read_info(png_ptr, info_ptr);

  width = png_get_image_width(png_ptr, info_ptr);
  height = png_get_image_height(png_ptr, info_ptr);
  color_type = png_get_color_type(png_ptr, info_ptr);
  bit_depth = png_get_bit_depth(png_ptr, info_ptr);

  number_of_passes = png_set_interlace_handling(png_ptr);
  png_read_update_info(png_ptr, info_ptr);

  /* read file */
  if (setjmp(png_jmpbuf(png_ptr)))
    abort_("[read_png_file] Error during read_image");

  row_pointers = (png_bytep *)malloc(sizeof(png_bytep) * height);
  for (y = 0; y < height; y++)
    row_pointers[y] = (png_byte *)malloc(png_get_rowbytes(png_ptr, info_ptr));

  png_read_image(png_ptr, row_pointers);

  fclose(fp);
}

void write_png_file(char *file_name)
{
  /* create file */
  FILE *fp = fopen(file_name, "wb");
  if (!fp)
    abort_("[write_png_file] File %s could not be opened for writing", file_name);

  /* initialize stuff */
  png_ptr = png_create_write_struct(PNG_LIBPNG_VER_STRING, NULL, NULL, NULL);

  if (!png_ptr)
    abort_("[write_png_file] png_create_write_struct failed");

  info_ptr = png_create_info_struct(png_ptr);
  if (!info_ptr)
    abort_("[write_png_file] png_create_info_struct failed");

  if (setjmp(png_jmpbuf(png_ptr)))
    abort_("[write_png_file] Error during init_io");

  png_init_io(png_ptr, fp);

  /* write header */
  if (setjmp(png_jmpbuf(png_ptr)))
    abort_("[write_png_file] Error during writing header");

  png_set_IHDR(png_ptr, info_ptr, width, height,
               bit_depth, color_type, PNG_INTERLACE_NONE,
               PNG_COMPRESSION_TYPE_BASE, PNG_FILTER_TYPE_BASE);

  png_write_info(png_ptr, info_ptr);

  /* write bytes */
  if (setjmp(png_jmpbuf(png_ptr)))
    abort_("[write_png_file] Error during writing bytes");

  png_write_image(png_ptr, row_pointers);

  /* end write */
  if (setjmp(png_jmpbuf(png_ptr)))
    abort_("[write_png_file] Error during end of write");

  png_write_end(png_ptr, NULL);

  /* cleanup heap allocation */
  for (y = 0; y < height; y++)
    free(row_pointers[y]);
  free(row_pointers);

  fclose(fp);
}

void hex2chars(char hex[], unsigned char result[])
{
  int len = strlen(hex);
  char temp[3];
  temp[2] = 0x00;

  int j = 0;
  for (int i = 0; i < len; i += 2)
  {
    temp[0] = hex[i];
    temp[1] = hex[i + 1];
    result[j] = (char)strtol(temp, NULL, 16);
    ++j;
  }
}

void process_file(int encrypt)
{
  if (png_get_color_type(png_ptr, info_ptr) == PNG_COLOR_TYPE_RGB)
    abort_("[process_file] input file is PNG_COLOR_TYPE_RGB but must be PNG_COLOR_TYPE_RGBA "
           "(lacks the alpha channel)");

  if (png_get_color_type(png_ptr, info_ptr) != PNG_COLOR_TYPE_RGBA)
    abort_("[process_file] color_type of input file must be PNG_COLOR_TYPE_RGBA (%d) (is %d)",
           PNG_COLOR_TYPE_RGBA, png_get_color_type(png_ptr, info_ptr));

  unsigned char k[100],
      t[100];

  hex2chars("EF4359D8D580AA4F7F036D6F04FC6A94", k); // values copied from example
  hex2chars("D8E7920AFA330A73", t);                 // values copied from example

  int klen = strlen("EF4359D8D580AA4F7F036D6F04FC6A94") / 2;
  int tlen = strlen("D8E7920AFA330A73") / 2;
  int radix = 256; // pixel value range

  FPE_KEY ff1;
  FPE_set_ff1_key(k, klen * 8, t, tlen, radix, &ff1);

  int block_width_size = width / NUM_BLOCKS;
  int block_height_size = height / NUM_BLOCKS;

  double encrypt_percent = 0.95;
  int num_rounds = (int)(NUM_BLOCKS * NUM_BLOCKS * encrypt_percent);

  srand(24); // set random seed to allow for decryption

  int index_used[NUM_BLOCKS][NUM_BLOCKS] = { 0 };

  for (int i = 0; i < num_rounds; ++i)
  {

    int width_index = rand() % NUM_BLOCKS;
    int height_index = rand() % NUM_BLOCKS;
    while (index_used[width_index][height_index])
    {
      width_index = rand() % NUM_BLOCKS;
      height_index = rand() % NUM_BLOCKS;
    }
    index_used[width_index][height_index] = 1;

    int start_width = (rand() % NUM_BLOCKS) * block_width_size;
    int start_height = (rand() % NUM_BLOCKS) * block_height_size;

    int end_width = start_width + block_width_size;
    int end_height = start_height + block_height_size;
    
    for (y = start_height; y < end_height; y++)
    {
      png_byte *row = row_pointers[y];
      for (x = start_width; x < end_width; x++)
      {
        png_byte *ptr = &(row[x * 4]);

        unsigned int x[4];
        unsigned int y[4];
        for (int i = 0; i < 4; ++i)
        {
          x[i] = ptr[i];
        }
        if (encrypt)
          FPE_ff1_encrypt(x, y, 4, &ff1, FPE_ENCRYPT);
        else
        {
          FPE_ff1_encrypt(x, y, 4, &ff1, FPE_DECRYPT);
        }
        for (int i = 0; i < 4; ++i)
        {
          ptr[i] = y[i];
        }
      }
    }
  }
}

int main(int argc, char **argv)
{
  if (argc != 4)
    abort_("Usage: program_name <encrypt_bool> <file_in> <file_out>");

  read_png_file(argv[2]);
  process_file(atoi(argv[1]));
  write_png_file(argv[3]);

  return 0;
}