from pypdf import PdfReader

reader = PdfReader('Cau_Hoi_On_Tap.pdf')
with open('Cau_Hoi_On_Tap.txt', 'w', encoding='utf-8') as out:
    out.write(f'pages: {len(reader.pages)}\n')
    for i, page in enumerate(reader.pages):
        out.write(f'\n--- PAGE {i + 1} ---\n')
        out.write(page.extract_text() or '')
