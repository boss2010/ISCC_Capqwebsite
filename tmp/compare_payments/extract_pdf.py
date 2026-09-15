import json
import re
import pdfplumber

path = r"C:\Users\mohamed.lotfy\Downloads\search2026-08-06T13_57_45.999+03_00.pdf"
pages = []
orders = []
with pdfplumber.open(path) as pdf:
    for i, page in enumerate(pdf.pages, 1):
        text = page.extract_text() or ""
        pages.append({"page": i, "text": text})
        for line in text.splitlines():
            m = re.match(r"^AGRICULTURE2\s+([0-9a-fA-F]+)\s+(?:[0-9a-fA-F]+\s+)?Captured\s+(\d{1,2}/\d{1,2}/\d{4})\s+(\d{1,2}:\d{2})\s+([\d,]+(?:\.\d+)?)\s*$", line.strip())
            if m:
                orders.append({"page": i, "order_id": m.group(1).lower(), "date": m.group(2), "time": m.group(3), "amount": float(m.group(4).replace(",", "")), "line": line})
with open(r"tmp\compare_payments\pdf_extract.json", "w", encoding="utf-8") as f:
    json.dump({"page_count": len(pages), "orders": orders, "pages": pages}, f, ensure_ascii=False, indent=2)
print(json.dumps({"page_count": len(pages), "order_count": len(orders), "orders_by_page": {str(p): sum(1 for x in orders if x["page"] == p) for p in range(1, len(pages)+1)}}, ensure_ascii=False))
