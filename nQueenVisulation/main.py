import pandas as pd
import matplotlib.pyplot as plt

# İlk dosyadaki verileri yükle
df_parallel = pd.read_csv("saveSolutionCountAndTime.csv")

# İkinci dosyadaki verileri yükle
df_sequential = pd.read_csv("saveSolutionCountAndTimeSequential.csv")

# Verileri birleştirerek tek bir DataFrame oluştur
df = pd.concat([df_parallel, df_sequential], axis=1)
df.columns = ['Vezir Sayısı', 'Çözüm Sayısı (Paralel)', 'Geçen Süre (Paralel)', 'Vezir Sayısı (Sıralı)', 'Çözüm Sayısı (Sıralı)', 'Geçen Süre (Sıralı)']

# Süreleri karşılaştırmak için çizgi grafiği oluştur
plt.plot(df['Vezir Sayısı'], df['Geçen Süre (Paralel)'], marker='o', label='Paralel')
plt.plot(df['Vezir Sayısı (Sıralı)'], df['Geçen Süre (Sıralı)'], marker='o', label='Sıralı')

# Grafiği düzenleme
plt.xlabel('Vezir Sayısı')
plt.ylabel('Geçen Süre (ms)')
plt.title('Paralel ve Sıralı Çözüm Süreleri Karşılaştırması')
plt.legend()
plt.grid(True)

# Grafiği göster
plt.show()
