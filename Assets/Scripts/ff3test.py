from fpe.h import FF3Cipher
import base64

key = "2DE79D232DF5585D68CE47882AE256D6"
tweak = "CBD09280979564"
c = FF3Cipher(key, tweak, "UTF-8")


with open("Assets\Scripts\PAtestimage.jpg", "rb") as image:
  f = base64.b64encode(image.read()).decode('utf-8')

plaintext = f
ciphertext = c.encrypt(plaintext)
decrypted = c.decrypt(ciphertext)

print(f"{plaintext} -> {ciphertext} -> {decrypted}")

# format encrypted value
ccn = f"{ciphertext[:4]} {ciphertext[4:8]} {ciphertext[8:12]} {ciphertext[12:]}"
print(f"Encrypted CCN value with formatting: {ccn}")