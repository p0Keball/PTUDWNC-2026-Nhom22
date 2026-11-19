# -*- coding: utf-8 -*-
"""
Script to generate Nhom22_BaoCao_PTUDWEBNC.docx
Revised to strictly adhere to:
1. Academic Project Report Heading Standards (Quy cách & định dạng Báo cáo đồ án):
   - CHƯƠNG 1. TỔNG QUAN DỰ ÁN VÀ TIẾN TRÌNH TÍCH HỢP HỆ THỐNG
     - 1.1. Kiến trúc kỹ thuật tổng thể
     - 1.2. Tiến trình tích hợp từ Tuần 1 đến Tuần 5
   - CHƯƠNG 2. TỔNG HỢP CÁC BÀI LAB ĐÃ THỰC HIỆN VÀ BẢNG PHÂN CÔNG CÔNG VIỆC
     - 2.1. Bảng phân công thành viên và vai trò chính (Nhóm 22)
     - 2.2. Bảng tổng hợp chi tiết công việc từ Tuần 1 đến Tuần 5 (16 nhóm nhiệm vụ)
     - 2.3. Kết luận đánh giá tiến độ thực hiện
   - CHƯƠNG 3. HIỆN THỰC VÀ DEMO HỆ THỐNG
     - 3.1 -> 3.12: 12 màn hình demo với Khung ảnh URL/hướng dẫn, Mô tả kỹ thuật, Cách sử dụng
   - CHƯƠNG 4. HƯỚNG DẪN CÀI ĐẶT VÀ KHỞI CHẠY CHƯƠNG TRÌNH
     - 4.1. Yêu cầu môi trường cài đặt phần mềm
     - 4.2. Các bước cài đặt và khởi chạy chi tiết (4.2.1 -> 4.2.6)
   - CHƯƠNG 5. LIÊN KẾT REPOSITORY CHỨA MÃ NGUỒN VÀ CÁC NHÁNH TÍCH HỢP
     - 5.1. Kho chứa mã nguồn chính thức của dự án (GitHub)
     - 5.2. Nhánh tích hợp và các nhánh làm việc cá nhân của thành viên
2. Exact Cover Page from Nhom 14:
   - Double-line artistic page border (<w:pgBorders w:display="firstPage" w:offsetFrom="page"> val="basicWideMidline" sz="8" space="24")
   - Dalat University Logo (extracted_media/image1.png) centered
   - Clean professional metadata, lecturer and 4 students of Group 22
   - Section break separating cover page
3. Consistent academic formatting:
   - Margins: Left 3.0 cm, Right 2.0 cm, Top 2.0 cm, Bottom 2.0 cm
   - Fonts: Times New Roman (14pt bold H1, 13pt bold H2, 13pt bold-italic H3, 13pt regular body), Courier New 9.5pt for code
   - Line spacing: 1.35x throughout body
   - Paragraph indent: first_line_indent = 1.27 cm (0.5 inch = 720 dxa) for content; 0 for headings, tables, code
   - 0 soft line breaks in body text (strictly eliminating any word stretching)
4. Page count: 22-24 pages (Target: 20-25 pages).
"""

import os
import docx
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml import parse_xml, OxmlElement
from docx.oxml.ns import nsdecls, qn

def create_report():
    doc = Document()
    
    # -------------------------------------------------------------
    # 1. PAGE SETUP (SECTION 0 - COVER PAGE)
    # -------------------------------------------------------------
    sec0 = doc.sections[0]
    sec0.page_width = Pt(595.3)   # A4 width (210 mm)
    sec0.page_height = Pt(841.9)  # A4 height (297 mm)
    sec0.top_margin = Pt(56.9)    # 2.0 cm
    sec0.bottom_margin = Pt(56.9) # 2.0 cm
    sec0.left_margin = Pt(84.95)  # 3.0 cm
    sec0.right_margin = Pt(56.9)  # 2.0 cm
    
    # Configure exact double-line border for first page (matching Nhom 14)
    pgBorders = parse_xml(f'''
        <w:pgBorders {nsdecls("w")} w:display="firstPage" w:offsetFrom="page">
            <w:top w:val="basicWideMidline" w:sz="8" w:space="24" w:color="auto" />
            <w:left w:val="basicWideMidline" w:sz="8" w:space="24" w:color="auto" />
            <w:bottom w:val="basicWideMidline" w:sz="8" w:space="24" w:color="auto" />
            <w:right w:val="basicWideMidline" w:sz="8" w:space="24" w:color="auto" />
        </w:pgBorders>
    ''')
    sec0._sectPr.append(pgBorders)

    # Configure default style (Normal)
    style_normal = doc.styles['Normal']
    style_normal.font.name = 'Times New Roman'
    style_normal.font.size = Pt(13)
    style_normal.font.color.rgb = RGBColor(0, 0, 0)
    style_normal.paragraph_format.line_spacing = 1.35
    style_normal.paragraph_format.space_after = Pt(4)
    style_normal.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

    # XML helper functions
    def set_cell_background(cell, fill_hex):
        tcPr = cell._tc.get_or_add_tcPr()
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_hex}"/>')
        tcPr.append(shd)

    def set_cell_margins(cell, top=60, bottom=60, left=100, right=100):
        tcPr = cell._tc.get_or_add_tcPr()
        tcMar = parse_xml(f'''
            <w:tcMar {nsdecls("w")}>
                <w:top w:w="{top}" w:type="dxa"/>
                <w:bottom w:w="{bottom}" w:type="dxa"/>
                <w:left w:w="{left}" w:type="dxa"/>
                <w:right w:w="{right}" w:type="dxa"/>
            </w:tcMar>
        ''')
        tcPr.append(tcMar)

    def set_table_borders(table, color="E5E7EB", sz="4", val="single"):
        tblPr = table._tbl.tblPr
        tblBorders = parse_xml(f'''
            <w:tblBorders {nsdecls("w")}>
                <w:top w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:left w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:bottom w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:right w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:insideH w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:insideV w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
            </w:tblBorders>
        ''')
        tblPr.append(tblBorders)

    def add_page_number_to_footer(footer):
        p = footer.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.line_spacing = 1.0
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.first_line_indent = Inches(0)
        r = p.add_run()
        r.font.name = 'Times New Roman'
        r.font.size = Pt(11)
        r.font.color.rgb = RGBColor(0, 0, 0)
        fldChar1 = parse_xml(f'<w:fldChar {nsdecls("w")} w:fldCharType="begin"/>')
        instrText = parse_xml(f'<w:instrText {nsdecls("w")}>PAGE</w:instrText>')
        fldChar2 = parse_xml(f'<w:fldChar {nsdecls("w")} w:fldCharType="separate"/>')
        fldChar3 = parse_xml(f'<w:fldChar {nsdecls("w")} w:fldCharType="end"/>')
        r._r.append(fldChar1)
        r._r.append(instrText)
        r._r.append(fldChar2)
        r._r.append(fldChar3)

    def add_p(text, bold=False, italic=False, size=13, align=WD_ALIGN_PARAGRAPH.JUSTIFY, space_before=0, space_after=4, line_spacing=1.35, color=RGBColor(0,0,0), indent=True):
        """Add a paragraph. Content paragraphs have first_line_indent = 1.27 cm (0.5 in)."""
        p = doc.add_paragraph()
        p.alignment = align
        p.paragraph_format.space_before = Pt(space_before)
        p.paragraph_format.space_after = Pt(space_after)
        p.paragraph_format.line_spacing = line_spacing
        if indent:
            p.paragraph_format.first_line_indent = Inches(0.5)
        else:
            p.paragraph_format.first_line_indent = Inches(0)
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(size)
        run.bold = bold
        run.italic = italic
        run.font.color.rgb = color
        return p

    def add_h1(text):
        """Heading 1: Chapter level (CHƯƠNG 1, 2, 3...)"""
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(14)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.line_spacing = 1.35
        p.paragraph_format.first_line_indent = Inches(0)
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(14)
        run.bold = True
        run.font.color.rgb = RGBColor(0, 0, 0)
        return p

    def add_h2(text):
        """Heading 2: Section level (1.1, 1.2, 2.1...)"""
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(10)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.line_spacing = 1.35
        p.paragraph_format.first_line_indent = Inches(0)
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(13)
        run.bold = True
        run.font.color.rgb = RGBColor(0, 0, 0)
        return p

    def add_h3(text):
        """Heading 3: Subsection level (1.1.1, Mô tả kỹ thuật, Cách sử dụng)"""
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(6)
        p.paragraph_format.space_after = Pt(3)
        p.paragraph_format.line_spacing = 1.35
        p.paragraph_format.first_line_indent = Inches(0)
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(13)
        run.bold = True
        run.italic = True
        run.font.color.rgb = RGBColor(0, 0, 0)
        return p

    def add_code_block(lines):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        set_table_borders(tbl, color="E5E7EB", sz="4", val="single")
        cell = tbl.rows[0].cells[0]
        set_cell_background(cell, "F2F4F7")
        set_cell_margins(cell, top=80, bottom=80, left=140, right=140)
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.line_spacing = 1.15
        p.paragraph_format.first_line_indent = Inches(0)
        for i, line in enumerate(lines):
            if i > 0:
                p = cell.add_paragraph()
                p.alignment = WD_ALIGN_PARAGRAPH.LEFT
                p.paragraph_format.space_before = Pt(0)
                p.paragraph_format.space_after = Pt(0)
                p.paragraph_format.line_spacing = 1.15
                p.paragraph_format.first_line_indent = Inches(0)
            run = p.add_run(line)
            run.font.name = 'Courier New'
            run.font.size = Pt(9.5)
            run.font.color.rgb = RGBColor(0, 0, 0)
        sp = doc.add_paragraph()
        sp.paragraph_format.space_before = Pt(0)
        sp.paragraph_format.space_after = Pt(2)
        sp.paragraph_format.first_line_indent = Inches(0)

    def add_image_placeholder(title, screen_url, guide_text):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        set_table_borders(tbl, color="2563EB", sz="8", val="single")
        cell = tbl.rows[0].cells[0]
        set_cell_background(cell, "F8FAFC")
        set_cell_margins(cell, top=80, bottom=80, left=140, right=140)
        
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_before = Pt(2)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.first_line_indent = Inches(0)
        r0 = p.add_run(f"📷 [HÌNH ẢNH MINH HỌA: {title.upper()}]")
        r0.font.name = 'Times New Roman'
        r0.font.size = Pt(11.5)
        r0.bold = True
        r0.font.color.rgb = RGBColor(37, 99, 235)
        
        p2 = cell.add_paragraph()
        p2.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p2.paragraph_format.space_before = Pt(2)
        p2.paragraph_format.space_after = Pt(1)
        p2.paragraph_format.first_line_indent = Inches(0)
        r1 = p2.add_run("• Đường dẫn truy cập: ")
        r1.font.name = 'Times New Roman'
        r1.font.size = Pt(10.5)
        r1.bold = True
        r2 = p2.add_run(screen_url)
        r2.font.name = 'Courier New'
        r2.font.size = Pt(10)
        r2.font.color.rgb = RGBColor(37, 99, 235)
        
        p3 = cell.add_paragraph()
        p3.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p3.paragraph_format.space_before = Pt(1)
        p3.paragraph_format.space_after = Pt(2)
        p3.paragraph_format.first_line_indent = Inches(0)
        r3 = p3.add_run("• Hướng dẫn chụp ảnh: ")
        r3.font.name = 'Times New Roman'
        r3.font.size = Pt(10.5)
        r3.bold = True
        r4 = p3.add_run(guide_text)
        r4.font.name = 'Times New Roman'
        r4.font.size = Pt(10.5)
        r4.italic = True
        
        sp = doc.add_paragraph()
        sp.paragraph_format.space_before = Pt(0)
        sp.paragraph_format.space_after = Pt(2)
        sp.paragraph_format.first_line_indent = Inches(0)

    # -------------------------------------------------------------
    # 2. EXACT COVER PAGE CONTENT (MATCHING NHOM 14)
    # -------------------------------------------------------------
    p0 = doc.add_paragraph()
    p0.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p0.paragraph_format.space_before = Pt(0)
    p0.paragraph_format.space_after = Pt(12)
    p0.paragraph_format.first_line_indent = Inches(0)
    r = p0.add_run("TRƯỜNG ĐẠI HỌC ĐÀ LẠT")
    r.font.name = 'Times New Roman'
    r.font.size = Pt(16)
    r.bold = True

    p1 = doc.add_paragraph()
    p1.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p1.paragraph_format.space_before = Pt(0)
    p1.paragraph_format.space_after = Pt(45)
    p1.paragraph_format.first_line_indent = Inches(0)
    r = p1.add_run("KHOA CÔNG NGHỆ THÔNG TIN")
    r.font.name = 'Times New Roman'
    r.font.size = Pt(16)
    r.bold = True

    # University Logo
    logo_path = os.path.join('extracted_media', 'image1.png')
    p2 = doc.add_paragraph()
    p2.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p2.paragraph_format.space_before = Pt(0)
    p2.paragraph_format.space_after = Pt(45)
    p2.paragraph_format.first_line_indent = Inches(0)
    if os.path.exists(logo_path):
        run_logo = p2.add_run()
        run_logo.add_picture(logo_path, width=Pt(170), height=Pt(170))

    p3 = doc.add_paragraph()
    p3.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p3.paragraph_format.space_before = Pt(0)
    p3.paragraph_format.space_after = Pt(12)
    p3.paragraph_format.first_line_indent = Inches(0)
    r = p3.add_run("BÁO CÁO MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO")
    r.font.name = 'Times New Roman'
    r.font.size = Pt(16)
    r.bold = True

    p4 = doc.add_paragraph()
    p4.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p4.paragraph_format.space_before = Pt(0)
    p4.paragraph_format.space_after = Pt(50)
    p4.paragraph_format.first_line_indent = Inches(0)
    r = p4.add_run("ĐỀ TÀI: CULINARY-BLOG")
    r.font.name = 'Times New Roman'
    r.font.size = Pt(16)
    r.bold = True

    # Info block with exact tabs and spacing
    p5 = doc.add_paragraph()
    p5.alignment = WD_ALIGN_PARAGRAPH.LEFT
    p5.paragraph_format.line_spacing = 1.5
    p5.paragraph_format.space_after = Pt(0)
    p5.paragraph_format.first_line_indent = Inches(0)
    r1 = p5.add_run("\tGiảng viên hướng dẫn:\t")
    r1.font.name = 'Times New Roman'
    r1.font.size = Pt(13)
    r1.bold = True
    r2 = p5.add_run("ThS. Nguyễn Văn Phúc")
    r2.font.name = 'Times New Roman'
    r2.font.size = Pt(13)

    p6 = doc.add_paragraph()
    p6.alignment = WD_ALIGN_PARAGRAPH.LEFT
    p6.paragraph_format.line_spacing = 1.5
    p6.paragraph_format.space_after = Pt(0)
    p6.paragraph_format.first_line_indent = Inches(0)
    r1 = p6.add_run("\tSinh viên thực hiện:\t")
    r1.font.name = 'Times New Roman'
    r1.font.size = Pt(13)
    r1.bold = True
    r2 = p6.add_run("2312637 – Trần Gia Huy (Trưởng nhóm)")
    r2.font.name = 'Times New Roman'
    r2.font.size = Pt(13)

    p7 = doc.add_paragraph()
    p7.alignment = WD_ALIGN_PARAGRAPH.LEFT
    p7.paragraph_format.line_spacing = 1.5
    p7.paragraph_format.space_after = Pt(0)
    p7.paragraph_format.first_line_indent = Inches(0)
    r = p7.add_run("\t\t2312646 – Trần Quốc Khánh")
    r.font.name = 'Times New Roman'
    r.font.size = Pt(13)

    p8 = doc.add_paragraph()
    p8.alignment = WD_ALIGN_PARAGRAPH.LEFT
    p8.paragraph_format.line_spacing = 1.5
    p8.paragraph_format.space_after = Pt(0)
    p8.paragraph_format.first_line_indent = Inches(0)
    r = p8.add_run("\t\t2312677 – Ngô Văn Trường Long")
    r.font.name = 'Times New Roman'
    r.font.size = Pt(13)

    p9 = doc.add_paragraph()
    p9.alignment = WD_ALIGN_PARAGRAPH.LEFT
    p9.paragraph_format.line_spacing = 1.5
    p9.paragraph_format.space_after = Pt(55)
    p9.paragraph_format.first_line_indent = Inches(0)
    r = p9.add_run("\t\t2312635 – Trần Đình Việt Huy")
    r.font.name = 'Times New Roman'
    r.font.size = Pt(13)

    p11 = doc.add_paragraph()
    p11.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p11.paragraph_format.space_before = Pt(0)
    p11.paragraph_format.space_after = Pt(0)
    p11.paragraph_format.first_line_indent = Inches(0)
    r_date = p11.add_run("Đà Lạt, tháng 11 năm 2026")
    r_date.font.name = 'Times New Roman'
    r_date.font.size = Pt(13)
    r_date.bold = True
    r_date.italic = True

    # Page break to Section 1
    doc.add_section(docx.enum.section.WD_SECTION.NEW_PAGE)
    sec1 = doc.sections[1]
    # Remove inherited page border from Section 1 so border only appears on the cover page
    pgB_sec1 = sec1._sectPr.find('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}pgBorders')
    if pgB_sec1 is not None:
        sec1._sectPr.remove(pgB_sec1)

    sec1.top_margin = Pt(56.9)
    sec1.bottom_margin = Pt(56.9)
    sec1.left_margin = Pt(84.95)
    sec1.right_margin = Pt(56.9)
    add_page_number_to_footer(sec1.footer)

    # -------------------------------------------------------------
    # 3. CHƯƠNG 1: TỔNG QUAN DỰ ÁN VÀ TIẾN TRÌNH TÍCH HỢP HỆ THỐNG
    # -------------------------------------------------------------
    add_h1("CHƯƠNG 1. TỔNG QUAN DỰ ÁN VÀ TIẾN TRÌNH TÍCH HỢP HỆ THỐNG")
    
    add_p(
        "Hệ thống Culinary Blog là nền tảng trực tuyến phục vụ cộng đồng ẩm thực, cho phép các đầu bếp và người dùng tự do sáng tạo, "
        "đăng tải, khám phá và tìm kiếm các công thức nấu ăn chuyên nghiệp. Dự án được triển khai toàn diện dựa trên phương pháp luận "
        "phát triển phần mềm hiện đại, tuân thủ nghiêm ngặt theo tài liệu đặc tả SRS v1.0.0 (chuẩn IEEE 830 / ISO/IEC/IEEE 29148:2018). "
        "Hệ thống vận hành theo kiến trúc hướng dịch vụ độc lập API-Driven Architecture, phân tách hoàn toàn giữa máy chủ Backend và giao diện Frontend."
    )

    add_h2("1.1. Kiến trúc kỹ thuật tổng thể")

    add_p(
        "Backend (.NET 10 Minimal APIs & Clean Architecture): Tầng máy chủ áp dụng triệt để mô hình Clean Architecture 4 tầng gồm Domain, Application, Infrastructure và API. "
        "Tầng Domain (FoodBlog.Domain) là nhân lõi độc lập, chứa các thực thể nghiệp vụ (Recipe, Category, RecipeStep, RecipeIngredient, RecipeImage, ApplicationUser, RefreshToken), "
        "Owned Entity RecipeNutrition, Enums trạng thái, SlugHelper và Domain Exceptions, bảo đảm không phụ thuộc vào bất kỳ thư viện ngoài nào (CONS-001). "
        "Tầng Application (FoodBlog.Application) điều phối nghiệp vụ theo mẫu thiết kế CQRS kết hợp MediatR và FluentValidation Pipeline Behavior. "
        "Tầng Infrastructure (FoodBlog.Infrastructure) sử dụng EF Core 10 tương tác PostgreSQL 16, quản lý Redis Cache, lưu trữ tệp MinIO S3, Hangfire xử lý background jobs và Serilog ghi log có cấu trúc. "
        "Tầng Presentation (FoodBlog.API) cung cấp các endpoint RESTful Minimal APIs chuẩn hóa phiên bản /api/v1, tích hợp Rate Limiting, chuẩn lỗi ProblemDetails RFC 7807 và tài liệu Scalar UI tại /scalar/v1."
    )
    add_p(
        "Frontend (Next.js 16 App Router & React 19): Tầng giao diện phát triển bằng Next.js App Router, TypeScript, Tailwind CSS và hệ thống component shadcn/ui. "
        "Ứng dụng kết hợp linh hoạt giữa Static Generation với Incremental Static Regeneration (ISR) cho trang chủ và trang chi tiết món ăn; Server-Side Rendering (SSR) cho tìm kiếm đa tiêu chí; "
        "và Client-Side Rendering (CSR) cho Bảng điều khiển tác giả và Trang cá nhân. Quản lý trạng thái máy chủ bằng TanStack Query v5, quản lý trạng thái giao diện bằng Zustand, "
        "và kết nối HTTP qua Axios Client tích hợp Silent Token Refresh Interceptor tự động làm mới phiên làm việc khi gặp phản hồi 401 Unauthorized."
    )
    add_p(
        "Cơ sở dữ liệu & Tìm kiếm (PostgreSQL 16): EF Core Code-First Migrations quản lý toàn bộ lược đồ quan hệ. "
        "Hệ thống kích hoạt tiện ích mở rộng unaccent và pg_trgm. Cột SearchVector dạng tsvector được đồng bộ tự động từ Title và Description thông qua Database Trigger, "
        "kết hợp chỉ mục GIN cho phép tìm kiếm toàn văn (Full-Text Search) tiếng Việt không dấu siêu tốc, xếp hạng kết quả bằng ts_rank(). Tối ưu hóa truy vấn chuyên sâu với AsSplitQuery() và AsNoTracking()."
    )
    add_p(
        "Bộ nhớ đệm & Bảo mật (Redis 7 & JWT Identity): Áp dụng chiến lược lưu đệm đa tầng: IMemoryCache (TTL 60 phút) cho danh mục; "
        "Redis 7 Distributed Cache cho danh sách và chi tiết công thức (TTL 15 phút theo mô hình Cache-aside pattern). Hệ thống tự động xóa bộ đệm qua EvictByTagAsync ngay khi có thao tác thêm, sửa, xóa món ăn. "
        "Hệ thống phân quyền đa tầng gồm Role-Based Access Control (RBAC), Policy-based và Resource-based Authorization chống triệt để lỗ hổng BOLA (OWASP API1). "
        "Cơ chế phiên xác thực sử dụng JWT Access Token (15 phút) kết hợp Refresh Token Rotation SHA-256 (7 ngày) và thuật toán giám sát tính toàn vẹn của chuỗi token nhằm bảo vệ tối đa người dùng."
    )
    add_p(
        "Lưu trữ Đối tượng & Nhật ký Hệ thống (MinIO S3 & Serilog Audit Logging): Quản lý tệp đa phương tiện trên cụm lưu trữ tương thích chuẩn AWS S3 MinIO với cơ chế Fallback an toàn sang thư mục cục bộ. "
        "Hệ thống thẩm tra nghiêm ngặt Magic Bytes (JPEG, PNG, WebP, AVIF) và giới hạn dung lượng tải lên dưới 5MB. Hangfire job tự động tạo các biến thể ảnh medium (800x600) và thumbnail (300x300) bằng ImageSharp. "
        "Hạ tầng kiểm toán vận hành bởi EF Core AuditLoggingInterceptor tự động ghi nhận toàn bộ nhật ký Thêm, Sửa, Xóa và lưu vết biến động dữ liệu chi tiết dưới dạng JSONB."
    )

    add_h2("1.2. Tiến trình tích hợp từ Tuần 1 đến Tuần 5")

    add_p(
        "Dự án được triển khai theo mô hình làm việc nhóm phân tán chuẩn Git Flow qua 5 tuần phát triển liên tục, mỗi tuần tập trung giải quyết trọn vẹn một mảng nghiệp vụ chuyên biệt: "
        "Tuần 1 thiết lập nền tảng kiến trúc Clean Architecture, cấu hình PostgreSQL, Identity, MinIO và giao diện khung. "
        "Tuần 2 hiện thực toàn diện CRUD công thức ẩm thực, xử lý xung đột đồng thời Concurrency Token RowVersion, danh mục và bộ lọc. "
        "Tuần 3 tích hợp Google OAuth 2.0 PKCE, PostgreSQL Full-Text Search unaccent, Hangfire background jobs và tối ưu hóa bộ nhớ đệm Output Cache. "
        "Tuần 4 hoàn thiện hệ thống giám sát Observability (Serilog, OpenTelemetry, Health Checks), kiểm soát vòng đời bài viết, thư viện ảnh và tối ưu hiệu năng truy vấn. "
        "Tuần 5 hoàn thiện bộ kiểm thử tự động xUnit (Clean Architecture Tests, Unit Tests, Integration Tests) và chuẩn bị tài liệu bàn giao kỹ thuật."
    )
    add_p(
        "Quy trình phối hợp mã nguồn tuân thủ nghiêm ngặt kỷ luật phân nhánh: Bốn thành viên làm việc độc lập trên các nhánh tính năng cá nhân (GiaHuy, QKhanh, Tlong, BrVHuy). "
        "Tất cả các thay đổi đều được kiểm thử kỹ lưỡng cục bộ trước khi tạo Pull Request hợp nhất vào nhánh integration để thẩm định tương thích, "
        "và cuối cùng được merge thành công vào nhánh main duy nhất trên GitHub của dự án."
    )

    # -------------------------------------------------------------
    # 4. CHƯƠNG 2: TỔNG HỢP CÁC BÀI LAB ĐÃ THỰC HIỆN VÀ BẢNG PHÂN CÔNG CÔNG VIỆC
    # -------------------------------------------------------------
    doc.add_page_break()
    add_h1("CHƯƠNG 2. TỔNG HỢP CÁC BÀI LAB ĐÃ THỰC HIỆN VÀ BẢNG PHÂN CÔNG CÔNG VIỆC")

    # 2.1 Table 0: Team Members and Roles
    add_h2("2.1. Bảng phân công thành viên và vai trò chính (Nhóm 22)")
    
    t0_data = [
        ["STT", "Họ và tên", "MSSV", "Vai trò chính trong dự án"],
        ["1", "Trần Gia Huy", "2312637", "Trưởng nhóm; Phụ trách Child Data (Steps, Ingredients, Images); MinIO S3 File Storage Service; Background Jobs (Thumbnail Generation, Orphan Cleanup); Vòng đời trạng thái công thức (Status Lifecycle); Phân hệ Quản trị Admin & Audit Logging."],
        ["2", "Trần Quốc Khánh", "2312646", "Kiến trúc nền tảng (Architecture Foundation); Thiết lập .NET 10 Clean Architecture; CQRS với MediatR & FluentValidation; Recipe Core CRUD; Kiểm soát đồng thời Concurrency Control (RowVersion / ETag); Tối ưu hóa truy vấn EF Core 10; Output Cache; Metadata SEO JSON-LD Schema.org; Nền tảng giao diện Recipe Core trên Next.js."],
        ["3", "Ngô Văn Trường Long", "2312677", "Phụ trách Xác thực & Bảo mật (Authentication & Security); ASP.NET Core Identity; Quản lý phiên JWT Stateless & Refresh Token Rotation; Kiểm soát tính toàn vẹn phiên làm việc; Tích hợp Google OAuth 2.0 PKCE; Hangfire Welcome Email Job; Hệ thống Giám sát Observability (Serilog Structured Logging, OpenTelemetry Tracing, Health Checks); Giao diện Auth & Profile."],
        ["4", "Trần Đình Việt Huy", "2312635", "Phụ trách Phân loại & Tìm kiếm (Category & Search); Quản lý Danh mục Category CRUD; Tìm kiếm toàn văn PostgreSQL Full-Text Search (tsvector, tsquery, unaccent, GIN Index); Bộ lọc đa tiêu chí (Filter, Sort, Offset-based Pagination); Hangfire Sitemap.xml Recurring Job; Giao diện Tìm kiếm /search và Quản trị Danh mục /admin/categories."]
    ]

    t0 = doc.add_table(rows=len(t0_data), cols=4)
    t0.alignment = WD_TABLE_ALIGNMENT.CENTER
    set_table_borders(t0)
    col_widths = [Pt(35), Pt(115), Pt(65), Pt(238)]

    for r_idx, row in enumerate(t0.rows):
        for c_idx, cell in enumerate(row.cells):
            cell.width = col_widths[c_idx]
            set_cell_margins(cell, top=70, bottom=70, left=100, right=100)
            cell.text = t0_data[r_idx][c_idx]
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if c_idx in [0, 2] else WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.line_spacing = 1.15
            p.paragraph_format.space_before = Pt(1)
            p.paragraph_format.space_after = Pt(1)
            p.paragraph_format.first_line_indent = Inches(0)
            r = p.runs[0]
            r.font.name = 'Times New Roman'
            r.font.size = Pt(10.5)
            if r_idx == 0:
                set_cell_background(cell, "F9FAFB")
                r.bold = True
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            elif r_idx % 2 == 1:
                set_cell_background(cell, "FFFFFF")
            else:
                set_cell_background(cell, "F9FAFB")

    sp_t0 = doc.add_paragraph()
    sp_t0.paragraph_format.space_after = Pt(6)
    sp_t0.paragraph_format.first_line_indent = Inches(0)

    # 2.2 Detailed Task Allocation Table (16 rows)
    add_h2("2.2. Bảng tổng hợp chi tiết công việc từ Tuần 1 đến Tuần 5")
    add_p(
        "Dưới đây là bảng tổng hợp các module công việc đã thực hiện từ Tuần 1 đến Tuần 5 được phân chia cụ thể cho từng thành viên, "
        "chi tiết các bước kỹ thuật đã triển khai và kết quả nghiệm thu trên toàn hệ thống:"
    )

    t1_headers = ["STT", "Thành viên & Nhóm bài Lab", "Mã công việc & Tên nhiệm vụ cụ thể", "Chi tiết các bước kỹ thuật đã thực hiện", "Tiến độ"]
    
    tasks_data = [
        # Long (4 tasks)
        (
            "1",
            "Ngô Văn Trường Long (2312677)\nTuần 1: Auth & Observability Foundation",
            "FR-AUTH-001 -> 002\nFR-SEC-001 -> 002\nNFR-SEC-001",
            "• Thiết lập ASP.NET Core Identity với ApplicationUser kế thừa IdentityUser.\n"
            "• Xây dựng JwtService sinh Access Token (15 phút) và Refresh Token SHA-256 (7 ngày).\n"
            "• Viết ExceptionHandlingMiddleware chuẩn hóa lỗi ProblemDetails (RFC 7807).\n"
            "• Xây dựng CorrelationIdMiddleware phục vụ truy vết phân tán.\n"
            "• Xây dựng form Đăng nhập, Đăng ký và trang Profile trên Next.js 16.",
            "100% (Hoàn thành)"
        ),
        (
            "2",
            "Ngô Văn Trường Long (2312677)\nTuần 2: Core Auth, Token Rotation & Bảo mật",
            "FR-AUTH-004 -> 005\nNFR-SEC-002 -> 003",
            "• Hiện thực quy trình Refresh Token Rotation: cấp cặp token mới và vô hiệu token cũ.\n"
            "• Cài đặt cơ chế kiểm soát tính toàn vẹn của chuỗi token nhằm bảo vệ người dùng.\n"
            "• Xây dựng API Đăng xuất (Logout) thu hồi toàn bộ Refresh Token.\n"
            "• Cấu hình chính sách khóa tài khoản tự động khi đăng nhập sai quá 5 lần liên tiếp.\n"
            "• Tích hợp Axios Interceptor tự động làm mới token dưới client khi gặp mã 401.",
            "100% (Hoàn thành)"
        ),
        (
            "3",
            "Ngô Văn Trường Long (2312677)\nTuần 3: Google OAuth, Profile & Hangfire Email",
            "FR-AUTH-003, 006, 007\nFR-JOB-001",
            "• Tích hợp đăng nhập mạng xã hội Google OAuth 2.0 với chuẩn bảo mật PKCE.\n"
            "• Tự động liên kết tài khoản Google với tài khoản người dùng đã tồn tại trong CSDL.\n"
            "• Xây dựng API và giao diện Cập nhật Hồ sơ cá nhân (Profile Form, đổi Avatar).\n"
            "• Tích hợp Hangfire Dashboard và tạo Background Job gửi Welcome Email bất đồng bộ.",
            "100% (Hoàn thành)"
        ),
        (
            "4",
            "Ngô Văn Trường Long (2312677)\nTuần 4 - 5: Observability, Kiểm thử Auth & Bàn giao",
            "FR-OBS-001 -> 003\nNFR-MAINT-001 -> 004",
            "• Xây dựng bộ ba Health Checks (/health, /health/live, /health/ready) cho DB, Redis, MinIO.\n"
            "• Tích hợp Serilog Structured Logging ghi log JSON kèm CorrelationId ra Console, File, Seq.\n"
            "• Cấu hình OpenTelemetry thu thập Distributed Tracing và Performance Metrics.\n"
            "• Viết bộ Integration Tests kiểm thử toàn bộ luồng Auth, Rotation, Lockout và bàn giao.",
            "100% (Hoàn thành)"
        ),
        # Việt Huy (4 tasks)
        (
            "5",
            "Trần Đình Việt Huy (2312635)\nTuần 1: Category Database & Khởi tạo Tìm kiếm",
            "FR-CAT-001 -> 002\nFR-DOM-003\nNFR-PERF-003",
            "• Thiết kế thực thể Category (Id, Name, Slug, Description, ImageUrl, DisplayOrder).\n"
            "• Cấu hình Fluent API quan hệ 1-N giữa Category và Recipe trong EF Core DbContext.\n"
            "• Kích hoạt tiện ích mở rộng unaccent và pg_trgm trên máy chủ cơ sở dữ liệu PostgreSQL.\n"
            "• Xây dựng API GET /api/v1/categories kèm IMemoryCache với thời gian sống TTL 60 phút.\n"
            "• Xây dựng layout danh mục và khung hiển thị Skeleton Loading trên Next.js.",
            "100% (Hoàn thành)"
        ),
        (
            "6",
            "Trần Đình Việt Huy (2312635)\nTuần 2: Quản trị Danh mục CRUD & Ràng buộc Xóa",
            "FR-CAT-003 -> 005\nNFR-REL-002",
            "• Xây dựng API Tạo danh mục (Create Category) với thuật toán Slugify tự động.\n"
            "• Xây dựng API Cập nhật danh mục và thiết lập cơ chế xóa đệm (Cache Invalidation).\n"
            "• Hiện thực ràng buộc toàn vẹn khi Xóa: chặn xóa nếu danh mục vẫn còn công thức liên kết.\n"
            "• Áp dụng chính sách bảo vệ Role-Based Authorization chỉ cho phép Admin thao tác CRUD.\n"
            "• Xây dựng giao diện Quản trị Danh mục (/dashboard/categories) thêm/sửa/xóa và tải ảnh bìa.",
            "100% (Hoàn thành)"
        ),
        (
            "7",
            "Trần Đình Việt Huy (2312635)\nTuần 3: Full-Text Search, Bộ lọc & Sitemap Job",
            "FR-SRCH-001 -> 004\nFR-JOB-003",
            "• Tạo cột SearchVector (tsvector) đồng bộ tự động qua PostgreSQL Database Trigger.\n"
            "• Cấu hình GIN Index tối ưu hóa truy vấn tìm kiếm toàn văn tiếng Việt không dấu.\n"
            "• Xây dựng API Tìm kiếm và lọc đa tiêu chí (danh mục, độ khó, thời gian) xếp hạng bằng ts_rank().\n"
            "• Cấu hình Hangfire Recurring Job tự động quét và tạo tệp sitemap.xml định kỳ.\n"
            "• Xây dựng giao diện Tìm kiếm (/search) có bộ lọc, sắp xếp và phân trang tức thì.",
            "100% (Hoàn thành)"
        ),
        (
            "8",
            "Trần Đình Việt Huy (2312635)\nTuần 4 - 5: Tối ưu Tìm kiếm, Test Category & Bàn giao",
            "NFR-PERF-001\nNFR-MAINT-002",
            "• Benchmark hiệu năng tìm kiếm Full-Text Search đạt thời gian phản hồi p95 < 150ms.\n"
            "• Thẩm tra các kịch bản tìm kiếm không dấu phức tạp (ví dụ: 'pho bo' tìm chính xác 'Phở bò').\n"
            "• Hoàn thiện các trạng thái giao diện tìm kiếm: Loading State, Empty State, Error State.\n"
            "• Viết Integration Tests kiểm thử chức năng Danh mục, Tìm kiếm và bàn giao tài liệu kỹ thuật.",
            "100% (Hoàn thành)"
        ),
        # Quốc Khánh (4 tasks)
        (
            "9",
            "Trần Quốc Khánh (2312646)\nTuần 1: Clean Architecture & Recipe Core",
            "FR-BKE-001 -> 004\nFR-RCP-001 -> 003\nCONS-001",
            "• Khởi tạo khung dự án Clean Architecture 4 tầng chuẩn mực trên nền tảng .NET 10.\n"
            "• Tích hợp thư viện MediatR cho CQRS và FluentValidation Pipeline Behavior.\n"
            "• Thiết kế thực thể Recipe cốt lõi, Owned Entity RecipeNutrition và kiểm soát đồng thời RowVersion.\n"
            "• Xây dựng CreateRecipeCommand, GetRecipesQuery (phân trang) và GetRecipeBySlugQuery.\n"
            "• Xây dựng giao diện Trang chủ (/) và Trang thư viện công thức (/recipes) trên Next.js.",
            "100% (Hoàn thành)"
        ),
        (
            "10",
            "Trần Quốc Khánh (2312646)\nTuần 2: Recipe Update, Xử lý Concurrency & EF Core",
            "FR-RCP-004\nNFR-REL-001\nNFR-PERF-002",
            "• Xây dựng API Cập nhật công thức và kiểm tra mã RowVersion xử lý xung đột đồng thời.\n"
            "• Trả về mã lỗi HTTP 409 Conflict chuẩn ProblemDetails khi xảy ra tranh chấp dữ liệu.\n"
            "• Cài đặt Resource-Based Authorization kiểm tra quyền sở hữu bài viết chống lỗ hổng BOLA.\n"
            "• Tối ưu hóa câu truy vấn EF Core 10 với Include, ThenInclude và AsSplitQuery tránh lỗi N+1.\n"
            "• Xây dựng giao diện chỉnh sửa công thức có hộp thoại thông báo xung đột dữ liệu.",
            "100% (Hoàn thành)"
        ),
        (
            "11",
            "Trần Quốc Khánh (2312646)\nTuần 3: Vòng đời Công thức, Output Cache & Dashboard",
            "FR-RCP-005 -> 007\nNFR-PERF-003\nFR-FTE-010",
            "• Xây dựng luồng Xuất bản công thức (Publish) kiểm tra điều kiện bắt buộc có ít nhất 1 bước nấu.\n"
            "• Hiện thực chức năng Lưu trữ (Archive) và Xóa mềm (Soft Delete, IsDeleted = true).\n"
            "• Tích hợp Output Cache cho các API công khai với tag 'recipes' và cơ chế xóa đệm theo sự kiện.\n"
            "• Xây dựng giao diện Bảng điều khiển tác giả (/dashboard/recipes) quản lý các tab trạng thái.",
            "100% (Hoàn thành)"
        ),
        (
            "12",
            "Trần Quốc Khánh (2312646)\nTuần 4 - 5: SEO JSON-LD, Kiến trúc Test & Bàn giao",
            "FR-SEO-001 -> 002\nNFR-MAINT-001 -> 002",
            "• Tích hợp siêu dữ liệu OpenGraph và Schema.org Recipe JSON-LD tối ưu hóa công cụ tìm kiếm.\n"
            "• Kiểm tra và tối ưu hóa các chỉ số hiệu năng trải nghiệm Core Web Vitals (LCP, CLS, INP).\n"
            "• Viết bộ kiểm thử kiến trúc CleanArchitectureTests.cs thẩm tra 5 ràng buộc bất biến qua Reflection.\n"
            "• Viết bộ Integration Tests kiểm tra vòng đời công thức, tải trọng p95/p99 và hoàn thiện bàn giao.",
            "100% (Hoàn thành)"
        ),
        # Gia Huy (4 tasks)
        (
            "13",
            "Trần Gia Huy (2312637)\nTuần 1: Recipe Child Data, MinIO S3 & Upload Ảnh",
            "FR-DOM-004 -> 006\nFR-INF-003\nCONS-007",
            "• Thiết kế các thực thể phụ thuộc RecipeStep, RecipeIngredient và RecipeImage trong EF Core.\n"
            "• Cấu hình dịch vụ lưu trữ đám mây MinIO S3 Object Storage qua Docker Compose.\n"
            "• Xây dựng dịch vụ IFileStorageService thực thi các thao tác tải lên và quản lý bucket MinIO.\n"
            "• Xây dựng API POST /api/v1/recipes/{id}/images tiếp nhận tệp đa phương tiện.\n"
            "• Xây dựng linh kiện Image Uploader có thanh tiến trình và biểu mẫu động thêm nguyên liệu/bước.",
            "100% (Hoàn thành)"
        ),
        (
            "14",
            "Trần Gia Huy (2312637)\nTuần 2: Quản lý Nguyên liệu, Các bước & Thẩm tra File",
            "FR-RCP-009 -> 010\nNFR-SEC-004\nCONS-007",
            "• Hoàn thiện trọn bộ API Thêm, Sửa, Xóa Nguyên liệu (RecipeIngredient) theo mã món ăn.\n"
            "• Hiện thực trọn bộ API Thêm, Sửa, Xóa Bước nấu (RecipeStep) tự động đánh lại số thứ tự StepNumber.\n"
            "• Xây dựng bộ thẩm định tệp nghiêm ngặt: kiểm tra MIME type, Magic Bytes và dung lượng tối đa 5MB.\n"
            "• Xây dựng biểu mẫu thêm công thức đa bước (Wizard) có hỗ trợ kéo thả đổi thứ tự các bước.",
            "100% (Hoàn thành)"
        ),
        (
            "15",
            "Trần Gia Huy (2312637)\nTuần 3: Gallery Ảnh, Thumbnail Job & Dọn dẹp MinIO",
            "FR-RCP-008\nFR-JOB-002\nNFR-REL-003",
            "• Xây dựng tính năng thiết lập ảnh đại diện chính (Set Primary Image) và Thư viện ảnh (Gallery).\n"
            "• Xây dựng Hangfire Background Job tự động tạo ảnh thu nhỏ (Thumbnail 300x300, Medium 800x600).\n"
            "• Hiện thực cơ chế dọn dẹp đệ quy (Recursive Cleanup) xóa sạch tệp trên MinIO khi xóa ảnh hoặc món.\n"
            "• Xây dựng giao diện xem trước ảnh tức thì (Image Preview) và quản lý bộ sưu tập ảnh món ăn.",
            "100% (Hoàn thành)"
        ),
        (
            "16",
            "Trần Gia Huy (2312637)\nTuần 4 - 5: MinIO Chịu lỗi, Quét File Mồ côi, Test & Bàn giao",
            "NFR-REL-002 -> 003\nFR-DVO-007\nNFR-MAINT-002",
            "• Cài đặt cơ chế chịu lỗi linh hoạt Fallback sang lưu trữ tệp cục bộ khi dịch vụ MinIO gặp sự cố.\n"
            "• Xây dựng Background Job tự động quét và dọn dẹp các tệp ảnh mồ côi (Orphaned Files) định kỳ.\n"
            "• Xây dựng AuditLoggingInterceptor ghi nhận toàn bộ biến động dữ liệu Thêm/Sửa/Xóa dạng JSONB.\n"
            "• Viết bộ Integration Tests kiểm thử chức năng lưu trữ tệp, xác thực định dạng và hoàn thành bàn giao.",
            "100% (Hoàn thành)"
        )
    ]

    t1 = doc.add_table(rows=len(tasks_data)+1, cols=5)
    t1.alignment = WD_TABLE_ALIGNMENT.CENTER
    set_table_borders(t1)
    t1_widths = [Pt(30), Pt(105), Pt(95), Pt(190), Pt(55)]

    # Header row
    for c_idx, h_text in enumerate(t1_headers):
        cell = t1.rows[0].cells[c_idx]
        cell.width = t1_widths[c_idx]
        set_cell_background(cell, "F9FAFB")
        set_cell_margins(cell, top=70, bottom=70, left=90, right=90)
        cell.text = h_text
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.line_spacing = 1.15
        p.paragraph_format.space_before = Pt(1)
        p.paragraph_format.space_after = Pt(1)
        p.paragraph_format.first_line_indent = Inches(0)
        r = p.runs[0]
        r.font.name = 'Times New Roman'
        r.font.size = Pt(10.5)
        r.bold = True

    # Data rows
    for r_idx, row_data in enumerate(tasks_data):
        row = t1.rows[r_idx + 1]
        for c_idx, val in enumerate(row_data):
            cell = row.cells[c_idx]
            cell.width = t1_widths[c_idx]
            if (r_idx // 4) % 2 == 1:
                set_cell_background(cell, "FFFFFF")
            else:
                set_cell_background(cell, "F9FAFB")
            set_cell_margins(cell, top=60, bottom=60, left=80, right=80)
            cell.text = val
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if c_idx in [0, 4] else WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.line_spacing = 1.15
            p.paragraph_format.space_before = Pt(1)
            p.paragraph_format.space_after = Pt(1)
            p.paragraph_format.first_line_indent = Inches(0)
            r = p.runs[0]
            r.font.name = 'Times New Roman'
            r.font.size = Pt(9.5)
            if c_idx == 4:
                r.bold = True
                r.font.color.rgb = RGBColor(22, 101, 52)  # green

    sp_t1 = doc.add_paragraph()
    sp_t1.paragraph_format.space_after = Pt(6)
    sp_t1.paragraph_format.first_line_indent = Inches(0)

    # 2.3 Evaluation and Conclusion
    add_h2("2.3. Kết luận đánh giá tiến độ thực hiện")
    add_p(
        "Toàn bộ 16 nhóm công việc lớn bao quát từ Tuần 1 đến Tuần 5 đã được 4 thành viên hoàn thành 100% khối lượng đề ra. "
        "Mọi yêu cầu nghiệp vụ trong tài liệu đặc tả SRS v1.0.0 đều được hiện thực trọn vẹn và kiểm chứng nghiêm ngặt."
    )
    add_p(
        "Tình trạng các chức năng: Hoàn thành đầy đủ tất cả các tính năng cốt lõi và nâng cao, không có chức năng nào bị bỏ dở hoặc chưa hoàn thành. "
        "Hệ thống kiểm soát đồng thời Concurrency Token, tìm kiếm toàn văn tiếng Việt không dấu và cơ chế phiên an toàn đều vận hành trơn tru."
    )
    add_p(
        "Độ ổn định hệ thống: Mã nguồn biên dịch thành công 100% trên cả Backend .NET 10 và Frontend Next.js 16; "
        "vượt qua toàn bộ 75+ bài kiểm thử tự động (gồm bộ kiểm thử kiến trúc Clean Architecture Tests, Unit Tests và Integration Tests)."
    )

    # -------------------------------------------------------------
    # 5. CHƯƠNG 3: HIỆN THỰC VÀ DEMO HỆ THỐNG
    # -------------------------------------------------------------
    doc.add_page_break()
    add_h1("CHƯƠNG 3. HIỆN THỰC VÀ DEMO HỆ THỐNG")

    demos = [
        {
            "sec": "3.1",
            "title": "Màn hình Đăng nhập tích hợp tiện ích Demo 1-Click đa vai trò (Admin, Author, User)",
            "url": "http://localhost/auth/login",
            "guide": "Chụp giao diện trang Đăng nhập (/auth/login): form đăng nhập email/mật khẩu, các nút đăng nhập và khung tiện ích DEMO 1-CLICK đa vai trò (Admin, Author) ngay bên dưới form.",
            "desc": [
                "Cổng xác thực danh tính người dùng an toàn được phát triển trên Next.js App Router kết hợp Backend ASP.NET Core Identity (FR-AUTH-002, FR-AUTH-003). "
                "Hệ thống hỗ trợ đầy đủ các phương thức: đăng nhập Email và Mật khẩu truyền thống với thuật toán băm bảo mật PBKDF2 theo chuẩn NFR-SEC-001 "
                "và cơ chế cấp phát Access Token (JWT) cùng Refresh Token an toàn trong cơ sở dữ liệu.",
                "Đặc biệt, nhằm tạo điều kiện thuận lợi nhất cho giảng viên và hội đồng kiểm tra tính năng, giao diện cung cấp tiện ích 'DEMO 1-CLICK' "
                "cho phép đăng nhập tức thì vào các vai trò được cấp quyền trước mà không cần nhập liệu thủ công:",
                "Tác giả ẩm thực (Author): Quyền tác giả sáng tạo nội dung, đăng tải và quản lý công thức cá nhân, chỉnh sửa và trải nghiệm chế độ nấu ăn.",
                "Quản trị viên (Admin): Quyền quản trị tối cao đối với toàn bộ danh mục món ăn, quản trị người dùng và giám sát nhật ký an ninh hệ thống."
            ],
            "steps": [
                "Người dùng mở trình duyệt web và truy cập địa chỉ http://localhost/auth/login (hoặc http://localhost:3000/auth/login).",
                "Đối với hình thức sử dụng tiện ích Demo 1-Click: Nhấp trực tiếp vào nút 'Admin (Quản trị)' hoặc 'Author (Tác giả)' ở khung phía dưới. Hệ thống sẽ tự động điền thông tin xác thực và chuyển hướng đến trang tương ứng chỉ trong 1 giây.",
                "Đối với hình thức đăng nhập thủ công: Nhập thông tin Email và Mật khẩu (ví dụ: admin@foodblog.com / Foodblog123!), sau đó nhấp vào nút 'Đăng nhập'.",
                "Nếu chưa có tài khoản, nhấp chọn liên kết 'Đăng ký' để chuyển đến biểu mẫu tạo tài khoản thành viên mới theo chuẩn FR-AUTH-001."
            ]
        },
        {
            "sec": "3.2",
            "title": "Trang Quản trị Người dùng dành cho Admin (/admin/users) - Phân quyền vai trò và Quản lý trạng thái tài khoản",
            "url": "http://localhost/admin/users",
            "guide": "Chụp giao diện Quản trị Người dùng (/admin/users): danh sách người dùng, thanh tìm kiếm, nhãn trạng thái tài khoản (Active, Suspended), vai trò (Admin, Author) và menu thao tác phân quyền.",
            "desc": [
                "Phân hệ quản trị danh tính nâng cao dành riêng cho Quản trị viên (Admin) tuân thủ chính sách phân quyền theo vai trò (Role-Based Access Control - RBAC). "
                "Bảng điều khiển cung cấp công cụ tìm kiếm tức thời theo tên hoặc email, hiển thị trực quan thông tin tài khoản, ngày tạo, vai trò và trạng thái hoạt động hiện tại (Active, Suspended).",
                "Tích hợp các thao tác quản trị đặc quyền gồm phân quyền vai trò (Author, Admin) và tạm ngưng hoặc mở khóa tài khoản người dùng ngay lập tức. "
                "Khi một tài khoản bị thay đổi quyền hoặc tạm ngưng, cơ chế Backend sẽ tự động thu hồi (Revoke) toàn bộ chuỗi Refresh Token trong cơ sở dữ liệu để chấm dứt phiên làm việc của người dùng đó trên mọi thiết bị."
            ],
            "steps": [
                "Quản trị viên đăng nhập hệ thống và điều hướng vào menu Dashboard rồi chọn mục Quản lý Người dùng tại địa chỉ http://localhost/admin/users.",
                "Nhập từ khóa vào thanh tìm kiếm ở đầu trang để lọc nhanh danh sách người dùng theo tên hoặc địa chỉ email cần kiểm tra.",
                "Để phân quyền vai trò: Nhấp vào nút tùy chọn tại cột Thao tác, chọn mục 'Phân quyền', tích chọn vai trò mong muốn (Author hoặc Admin) và nhấn nút 'Xác nhận'.",
                "Để tạm ngưng tài khoản: Nhấp chọn thao tác 'Khóa tài khoản' kèm nhập lý do vi phạm nếu có và xác nhận. Nhãn trạng thái tài khoản sẽ lập tức chuyển sang màu đỏ và người dùng bị vô hiệu hóa quyền truy cập."
            ]
        },
        {
            "sec": "3.3",
            "title": "Trang Xem và Cập nhật Hồ sơ Cá nhân (/profile) theo chuẩn FR-AUTH-006 và FR-AUTH-007",
            "url": "http://localhost/profile",
            "guide": "Chụp giao diện Hồ sơ cá nhân (/profile): thông tin cá nhân của người dùng đang đăng nhập, ảnh đại diện (avatar), email, tên hiển thị, tiểu sử (bio), ngày tham gia và biểu mẫu cập nhật thông tin.",
            "desc": [
                "Chức năng quản lý thông tin tài khoản cá nhân tuân thủ nghiêm ngặt đặc tả FR-AUTH-006 (Xem Hồ sơ Cá nhân) và FR-AUTH-007 (Cập nhật Hồ sơ Cá nhân) trong tài liệu SRS v1.0.0.",
                "Giao diện hiển thị trực quan thông tin chi tiết của người dùng: Họ và tên hiển thị, địa chỉ Email, ảnh đại diện lưu trữ trên MinIO S3, tiểu sử tóm tắt (Bio), vai trò hiện tại (Admin, Author, Member) và thời điểm đăng ký tài khoản.",
                "Cung cấp biểu mẫu cập nhật an toàn cho phép người dùng thay đổi Tên hiển thị, cập nhật Tiểu sử, và tải ảnh đại diện mới. Mọi thao tác cập nhật đều được kiểm tra ràng buộc chặt chẽ từ phía Client lẫn Server và ghi nhật ký kiểm toán hệ thống."
            ],
            "steps": [
                "Người dùng đăng nhập vào hệ thống bằng bất kỳ tài khoản nào (Admin, Author hoặc Member).",
                "Nhấp vào tên người dùng hoặc biểu tượng ảnh đại diện ở góc phải trên thanh điều hướng Navbar, hoặc truy cập trực tiếp đường dẫn http://localhost/profile.",
                "Quan sát thông tin hồ sơ hiển thị đầy đủ các trường dữ liệu cá nhân theo đặc tả FR-AUTH-006.",
                "Để cập nhật thông tin (FR-AUTH-007): Nhập tên hiển thị mới hoặc tiểu sử cá nhân và nhấn nút 'Lưu thay đổi'. Hệ thống sẽ tự động cập nhật thông tin và hiển thị thông báo thành công."
            ]
        },
        {
            "sec": "3.4",
            "title": "Trang Quản trị Danh mục Công thức cho Admin (/dashboard/categories) tích hợp tải ảnh bìa danh mục",
            "url": "http://localhost/dashboard/categories",
            "guide": "Chụp giao diện Quản trị Danh mục (/dashboard/categories): bảng danh mục ẩm thực, ảnh bìa danh mục, số lượng công thức liên kết, nút '+ Thêm danh mục' và các nút chỉnh sửa, xóa có kiểm tra ràng buộc.",
            "desc": [
                "Màn hình phục vụ Quản trị viên quản lý cấu trúc phân loại món ăn của toàn bộ hệ thống (FR-CAT-001 đến FR-CAT-005). "
                "Cung cấp trọn bộ tính năng CRUD danh mục: Thêm mới, Chỉnh sửa thông tin, tự động sinh Slug và Tải ảnh bìa danh mục trực tiếp lên cụm lưu trữ MinIO S3.",
                "Hệ thống tích hợp quy tắc bảo vệ toàn vẹn dữ liệu nghiêm ngặt: tự động đếm và hiển thị số lượng công thức đang thuộc về từng danh mục. "
                "Khi quản trị viên thực hiện thao tác xóa một danh mục đang có công thức liên kết, hệ thống sẽ tự động chặn thao tác và thông báo lý do rõ ràng theo chuẩn RFC 7807 ProblemDetails."
            ],
            "steps": [
                "Quản trị viên đăng nhập tài khoản Admin và truy cập vào Dashboard mục Danh mục tại địa chỉ http://localhost/dashboard/categories.",
                "Để thêm danh mục mới: Nhấp vào nút '+ Thêm danh mục', điền Tên danh mục, Mô tả ngắn, chọn tệp ảnh bìa từ máy tính tải lên MinIO S3 và nhấn 'Lưu'.",
                "Để chỉnh sửa danh mục: Nhấp vào biểu tượng chiếc bút tại hàng danh mục tương ứng, cập nhật lại thông tin hoặc thay thế ảnh bìa rồi nhấn 'Cập nhật'.",
                "Để xóa danh mục: Nhấp vào biểu tượng thùng rác. Nếu danh mục không chứa công thức nào, hộp thoại xác nhận sẽ cho phép xóa an toàn khỏi hệ thống."
            ]
        },
        {
            "sec": "3.5",
            "title": "Trang Quản trị Nhật ký Hệ thống (/admin/logs) hiển thị chi tiết Audit Log Diff (JSONB)",
            "url": "http://localhost/admin/logs",
            "guide": "Chụp giao diện Nhật ký hệ thống (/admin/logs): bảng nhật ký thao tác (Audit Logs), người thực hiện, thời gian, loại hành động (Create, Update, Delete) và hộp thoại hiển thị chi tiết biến động dữ liệu JSONB.",
            "desc": [
                "Bảng theo dõi và kiểm toán an ninh toàn diện cho hệ thống, vận hành bởi AuditLoggingInterceptor của Entity Framework Core theo chuẩn NFR-SEC-006 và FR-OBS-002. "
                "Hệ thống tự động ghi lại mọi thao tác Thêm (Create), Sửa (Update), Xóa (Delete) trên các bảng dữ liệu trọng yếu kèm thông tin người thực hiện, địa chỉ IP và dấu thời gian.",
                "Bảng nhật ký lưu vết chi tiết sự biến động dữ liệu giữa giá trị cũ (OldValues) và giá trị mới (NewValues) dưới dạng JSONB. "
                "Giao diện cho phép xem trực quan dạng Diff JSON, hỗ trợ đắc lực cho công tác hậu kiểm, phát hiện sai sót và nâng cao tính minh bạch trong quản trị."
            ],
            "steps": [
                "Quản trị viên đăng nhập quyền Admin, điều hướng vào Dashboard rồi chọn mục Nhật ký hệ thống tại http://localhost/admin/logs.",
                "Quan sát bảng tổng hợp các sự kiện gần nhất, sử dụng bộ lọc theo Hành động (Create, Update, Delete) hoặc theo Thực thể để tra cứu bản ghi cần kiểm tra.",
                "Nhấp vào nút 'Xem chi tiết (Diff JSON)' tại cột thao tác của từng dòng để mở hộp thoại hiển thị trực quan các trường dữ liệu trước và sau khi bị thay đổi."
            ]
        },
        {
            "sec": "3.6",
            "title": "Trang Bảng điều khiển Quản lý Công thức của Tác giả (/dashboard/recipes) - Quản lý Vòng đời qua Máy trạng thái",
            "url": "http://localhost:3000/dashboard/recipes",
            "guide": "Chụp giao diện Dashboard công thức của tác giả: thanh thống kê số lượng món ăn, các tab phân loại trạng thái (Tất cả, Bản nháp - Draft, Đã xuất bản - Published, Đã lưu trữ - Archived) kèm các nút thao tác nhanh (Chỉnh sửa, Xuất bản, Lưu trữ, Xóa).",
            "desc": [
                "Bảng điều khiển tác giả là trung tâm quản lý vòng đời toàn diện của công thức ẩm thực (FR-RCP-001, FR-RCP-005, FR-RCP-006, FR-RCP-007). "
                "Hệ thống kiểm soát trạng thái công thức qua mô hình máy trạng thái hữu hạn (State Machine) chặt chẽ.",
                "Trạng thái Bản nháp (Draft) là trạng thái mặc định khi khởi tạo, chỉ tác giả sở hữu và Quản trị viên nhìn thấy.",
                "Trạng thái Đã xuất bản (Published) là công thức được công khai ra cộng đồng và nạp vào chỉ mục tìm kiếm. Điều kiện bắt buộc để chuyển từ Draft sang Published là công thức phải có ít nhất một bước thực hiện (Steps.Count > 0), vi phạm sẽ bị chặn bởi RecipePublishIncompleteException.",
                "Trạng thái Đã lưu trữ (Archived) cho phép tạm ẩn công thức khỏi danh sách công khai mà không làm mất dữ liệu, có thể khôi phục lại trạng thái Bản nháp bằng endpoint /unarchive bất cứ lúc nào.",
                "Thao tác Xóa (Delete) áp dụng cơ chế xóa mềm (Soft Delete, IsDeleted = true) để bảo đảm toàn vẹn dữ liệu quan hệ theo chuẩn NFR-REL-003."
            ],
            "steps": [
                "Tác giả đăng nhập tài khoản và truy cập vào menu Dashboard rồi chọn mục Công thức của tôi tại http://localhost:3000/dashboard/recipes.",
                "Người dùng chuyển đổi linh hoạt giữa các tab Bản nháp, Đã xuất bản và Lưu trữ để lọc nhanh công thức theo nhu cầu quản trị.",
                "Tác giả nhấn nút Xuất bản để công khai món ăn, hệ thống sẽ tự động kiểm tra điều kiện có ít nhất một bước làm trước khi duyệt.",
                "Người dùng có thể nhấn Lưu trữ để ẩn món ăn tạm thời, hoặc nhấn Xóa kèm xác nhận để đưa bài viết vào trạng thái xóa mềm an toàn."
            ]
        },
        {
            "sec": "3.7",
            "title": "Trang Thêm mới Công thức đa bước (/dashboard/recipes/new) tích hợp tải ảnh MinIO và nhập 6 chỉ số dinh dưỡng",
            "url": "http://localhost:3000/dashboard/recipes/new",
            "guide": "Chụp giao diện Tạo công thức (/dashboard/recipes/new): biểu mẫu wizard nhập thông tin tổng quan, khung tải ảnh MinIO kéo thả, bảng nguyên liệu, các bước nấu và 6 chỉ số dinh dưỡng tiêu chuẩn.",
            "desc": [
                "Biểu mẫu khởi tạo công thức ẩm thực chuyên nghiệp theo mô hình Wizard đa bước, tích hợp cơ chế xác thực dữ liệu 2 đầu (Client-side Zod Validation và Server-side FluentValidation).",
                "Giao diện cho phép nhập thông tin tổng quan (Tên, mô tả, khẩu phần, thời gian chuẩn bị, thời gian nấu, độ khó), tải trực tiếp ảnh món ăn chất lượng cao lên MinIO S3 Object Storage với tính năng xem trước tức thì.",
                "Cho phép thêm động không giới hạn danh sách nguyên liệu và các bước thực hiện chi tiết có thể kéo thả đổi thứ tự. "
                "Đặc biệt, hệ thống tích hợp trường dữ liệu Owned Entity RecipeNutrition cho phép nhập 6 chỉ số dinh dưỡng tiêu chuẩn cho mỗi khẩu phần: Calo (Calories), Chất đạm (Protein), Chất béo (Fat), Tinh bột (Carbs), Chất xơ (Fiber) và Lượng đường (Sugar)."
            ],
            "steps": [
                "Tác giả đăng nhập tài khoản và chọn nút 'Tạo công thức' trên thanh điều hướng hoặc truy cập http://localhost:3000/dashboard/recipes/new.",
                "Điền thông tin cơ bản của món ăn và kéo thả tệp ảnh bìa món ăn vào khung tải ảnh MinIO S3 với định dạng hợp lệ.",
                "Bấm chuyển sang bước tiếp theo để thêm danh sách nguyên liệu và nhập đầy đủ 6 chỉ số dinh dưỡng cho khẩu phần ăn.",
                "Tiếp tục nhập từng bước thực hiện kèm thời gian nấu và ảnh minh họa cho từng công đoạn nếu có.",
                "Kiểm tra lại toàn bộ thông tin tại màn hình tổng kết và nhấn nút 'Lưu bản nháp' (Draft) hoặc 'Xuất bản công thức' (Publish) để hoàn tất."
            ]
        },
        {
            "sec": "3.8",
            "title": "Trang Danh mục & Khám phá Công thức (/recipes) có bộ lọc danh mục và sắp xếp đa tiêu chí",
            "url": "http://localhost:3000/recipes",
            "guide": "Chụp giao diện Thư viện công thức (/recipes): thanh bộ lọc bên trái (danh mục, độ khó, thời gian), hộp chọn sắp xếp (Mới nhất, Đánh giá cao nhất, Thời gian nhanh nhất), lưới thẻ món ăn và phân trang.",
            "desc": [
                "Kho thư viện công thức nấu ăn công khai của nền tảng với giao diện phân trang mượt mà được đồng bộ hóa tức thì bằng TanStack Query v5 mà không cần tải lại toàn bộ trang web.",
                "Thanh bộ lọc đa năng bên trái màn hình cho phép người dùng lọc nhanh món ăn theo Danh mục ẩm thực (Món Việt, Món Á, Món Âu, Món Chay), Mức độ khó (Dễ, Trung bình, Khó) và Khoảng thời gian chế biến.",
                "Hệ thống hỗ trợ sắp xếp kết quả linh hoạt theo: Mới nhất, Đánh giá cao nhất, Xem nhiều nhất, hoặc Thời gian nhanh nhất, giúp độc giả nhanh chóng tìm thấy món ăn phù hợp với thời gian và nguyên liệu sẵn có."
            ],
            "steps": [
                "Người dùng nhấp vào mục 'Công thức' trên thanh điều hướng chính hoặc truy cập địa chỉ http://localhost:3000/recipes.",
                "Tại bảng điều khiển bộ lọc bên trái, tích chọn danh mục món ăn mong muốn, độ khó phù hợp và khoảng thời gian nấu nướng dưới 30 phút hoặc 60 phút.",
                "Chọn tiêu chí sắp xếp tại hộp thả xuống ở góc phải (ví dụ: 'Mới nhất' hoặc 'Đánh giá cao nhất') để danh sách món ăn tự động cập nhật.",
                "Nhấp vào bất kỳ thẻ món ăn nào trong danh sách hiển thị để chuyển hướng đến trang xem chi tiết công thức và trải nghiệm chế độ nấu ăn."
            ]
        },
        {
            "sec": "3.9",
            "title": "Trang chủ Hệ thống Culinary Blog (/) với giao diện responsive hiện đại và bài viết nổi bật",
            "url": "http://localhost:3000/",
            "guide": "Chụp giao diện Trang chủ (/): banner trình chiếu món ăn nổi bật trong tuần, thanh tìm kiếm trung tâm, các huy hiệu danh mục thịnh hành, danh sách công thức mới nhất và bố cục tương thích màn hình.",
            "desc": [
                "Trang chủ đóng vai trò là bộ mặt của nền tảng ẩm thực Culinary Blog. Thiết kế giao diện theo xu hướng hiện đại, chuẩn Responsive thích ứng mượt mà từ màn hình điện thoại di động đến máy tính để bàn.",
                "Tích hợp thanh tìm kiếm thông minh ở vị trí trung tâm, banner trình chiếu các món ăn đặc sắc được yêu thích nhất trong tuần (Featured Recipes), thanh cuộn danh mục thịnh hành và khu vực bài viết công thức ẩm thực mới được xuất bản.",
                "Trang chủ áp dụng cơ chế Incremental Static Regeneration (ISR) với chu kỳ revalidate định kỳ giúp tốc độ tải trang gần như tức thì, mang lại trải nghiệm khám phá ẩm thực tối ưu cho người dùng."
            ],
            "steps": [
                "Người dùng mở trình duyệt web và truy cập vào địa chỉ gốc: http://localhost:3000/.",
                "Nhập từ khóa tìm kiếm món ăn hoặc nguyên liệu vào ô tìm kiếm trung tâm (hỗ trợ tìm tiếng Việt có dấu hoặc không dấu nhờ PostgreSQL unaccent FTS).",
                "Nhấp vào các huy hiệu danh mục nhanh như Món Khai Vị, Món Chính, Món Tráng Miệng để khám phá ngay các món ăn thuộc nhóm đó.",
                "Lướt xem các công thức tiêu biểu được giới thiệu và nhấp vào món ăn yêu thích để bắt đầu trải nghiệm nấu nướng."
            ]
        },
        {
            "sec": "3.10",
            "title": "Trang Chi tiết Công thức Nấu ăn (/recipes/[slug]) chuẩn đặc tả FR-RCP-002",
            "url": "http://localhost/recipes/rau-cau-dua-hoa-dau-biec-thanh-mat",
            "guide": "Chụp giao diện Chi tiết công thức: tiêu đề món ăn, thư viện ảnh (Recipe Gallery), thông tin nhanh (độ khó, thời gian nấu, khẩu phần), bảng nguyên liệu định lượng, các bước nấu và bảng dinh dưỡng.",
            "desc": [
                "Màn hình chi tiết công thức tuân thủ nghiêm ngặt đặc tả FR-RCP-002 trong tài liệu SRS v1.0.0. "
                "Hiển thị toàn diện cấu trúc dữ liệu của một bài viết ẩm thực chuẩn mực: tiêu đề, mô tả, thư viện ảnh đại diện MinIO S3, thông tin tác giả, thời gian chuẩn bị và thời gian nấu.",
                "Cung cấp danh sách nguyên liệu định lượng chi tiết với đơn vị tính chuẩn xác (RecipeIngredient) và các bước thực hiện từng công đoạn (RecipeStep).",
                "Hiển thị bảng 6 chỉ số dinh dưỡng tiêu chuẩn (RecipeNutrition): Calo (Calories), Chất đạm (Protein), Chất béo (Fat), Tinh bột (Carbohydrates), Chất xơ (Fiber) và Natri (Sodium).",
                "Trang được tối ưu hóa SEO với dữ liệu có cấu trúc chuẩn Schema.org Recipe JSON-LD (NFR-SEO-001) và cơ chế bộ nhớ đệm ISR revalidate 300 giây."
            ],
            "steps": [
                "Từ trang chủ hoặc trang danh sách công thức, người dùng nhấp chọn một món ăn bất kỳ (ví dụ: 'Rau Câu Dừa Hoa Đậu Biếc Thanh Mát') hoặc truy cập trực tiếp đường dẫn http://localhost/recipes/rau-cau-dua-hoa-dau-biec-thanh-mat.",
                "Quan sát tiêu đề món ăn, các huy hiệu thông tin nhanh gồm mức độ khó (Dễ), thời gian nấu (35 phút) và khẩu phần (6 người).",
                "Kiểm tra danh sách nguyên liệu định lượng chi tiết tại khung bên dưới.",
                "Theo dõi các bước thực hiện từng công đoạn và bảng phân tích chỉ số dinh dưỡng cân đối cho khẩu phần ăn."
            ]
        },
        {
            "sec": "3.11",
            "title": "Tài liệu API tương tác chuẩn OpenAPI 3.1 & Scalar UI tại đường dẫn /scalar/v1",
            "url": "http://localhost:8080/scalar/v1",
            "guide": "Chụp giao diện Scalar Interactive API Reference tại địa chỉ http://localhost:8080/scalar/v1: danh mục các nhóm endpoint (Auth, Recipes, Categories, Users, Logs, Health), thông tin schema dữ liệu chuẩn OpenAPI 3.1 và chức năng thử nghiệm API trực tiếp (Test Request).",
            "desc": [
                "Hệ thống Backend .NET 10 Minimal APIs tích hợp thư viện tài liệu hiện đại Scalar Interactive API Reference chuẩn OpenAPI 3.1 theo chuẩn NFR-MAINT-003.",
                "Cung cấp tài liệu kỹ thuật chi tiết cho toàn bộ hơn 40 endpoints thuộc các phân hệ: Auth, Users, Recipes, Categories, AuditLogs, Health và Media Upload. "
                "Scalar cung cấp giao diện thử nghiệm API trực tiếp (Interactive Sandbox), định nghĩa đầy đủ cấu trúc Request và Response Body, mã lỗi chuẩn ProblemDetails (RFC 7807) và cơ chế chèn Bearer Token kiểm tra phân quyền bảo mật."
            ],
            "steps": [
                "Khởi động máy chủ Backend và truy cập đường dẫn tài liệu: http://localhost:8080/scalar/v1.",
                "Duyệt qua cây danh mục API ở thanh bên trái để chọn Endpoint cần kiểm tra và đọc đặc tả dữ liệu.",
                "Nhấp vào nút 'Authorize' ở góc trên để dán chuỗi Bearer JWT Token đối với các API yêu cầu quyền xác thực.",
                "Nhấp nút 'Test Request', điền các tham số hoặc payload JSON mẫu và nhấn 'Send' để gửi yêu cầu và nhận kết quả phản hồi trực tiếp từ máy chủ."
            ]
        },
        {
            "sec": "3.12",
            "title": "Kết quả kiểm thử tự động toàn diện - 100% vượt qua (Clean Architecture Tests & Integration Tests)",
            "url": "Console Terminal / dotnet test",
            "guide": "Chụp màn hình cửa sổ dòng lệnh Terminal chạy lệnh dotnet test: hiển thị kết quả kiểm thử xanh lá cây toàn bộ 75+ bài test (CleanArchitectureTests, UnitTests, IntegrationTests) với thông báo 'Passed! Total tests: 75, Failed: 0'.",
            "desc": [
                "Minh chứng chất lượng và độ tin cậy cao của mã nguồn dự án thông qua hệ sinh thái kiểm thử tự động toàn diện được xây dựng bằng xUnit (NFR-MAINT-002, NFR-MAINT-004).",
                "Kiểm thử kiến trúc Clean Architecture (CleanArchitectureTests.cs) ứng dụng .NET Reflection độc lập để thẩm tra 5 ràng buộc bất biến: "
                "(1) Tầng Domain không phụ thuộc vào Application, Infrastructure hay Presentation; (2) Tầng Domain không tham chiếu thư viện ngoài; "
                "(3) Tầng Application không phụ thuộc vào Infrastructure; (4) Tầng Infrastructure tuân thủ nguyên lý Dependency Inversion; "
                "(5) Tất cả các thực thể cốt lõi đều kế thừa từ BaseEntity.",
                "Kiểm thử tích hợp (Integration Tests) sử dụng WebApplicationFactory kết hợp cơ sở dữ liệu EF Core InMemory riêng biệt, "
                "giả lập trọn vẹn luồng HTTP gọi API thật qua Client bao phủ toàn bộ các phân hệ nghiệp vụ chính.",
                "Kết quả nghiệm thu ghi nhận toàn bộ 75+ bài kiểm thử đều vượt qua 100% (Passed), bảo đảm hệ thống vận hành tin cậy và không xuất hiện bất kỳ lỗi hồi quy nào."
            ],
            "steps": [
                "Mở cửa sổ dòng lệnh Terminal hoặc PowerShell tại thư mục gốc của dự án.",
                "Thực thi câu lệnh chạy toàn bộ bài kiểm thử tự động: dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj --filter \"Category!=RequiresMinIO\".",
                "Quan sát kết quả tổng kết hiển thị trên màn hình: Toàn bộ các bài kiểm thử đều đạt trạng thái Passed màu xanh lá cây mà không có bất kỳ lỗi nào."
            ]
        }
    ]

    for d in demos:
        add_h2(f"{d['sec']}. {d['title']}")
        add_image_placeholder(d['title'], d['url'], d['guide'])
        add_h3("Mô tả kỹ thuật:")
        for desc_p in d['desc']:
            add_p(desc_p)
        add_h3("Cách sử dụng:")
        for step_p in d['steps']:
            add_p(step_p)

    # -------------------------------------------------------------
    # 6. CHƯƠNG 4: HƯỚNG DẪN CÀI ĐẶT VÀ KHỞI CHẠY CHƯƠNG TRÌNH
    # -------------------------------------------------------------
    doc.add_page_break()
    add_h1("CHƯƠNG 4. HƯỚNG DẪN CÀI ĐẶT VÀ KHỞI CHẠY CHƯƠNG TRÌNH")

    add_h2("4.1. Yêu cầu môi trường cài đặt phần mềm")
    add_p(
        "Để khởi chạy và kiểm thử toàn bộ hệ thống Culinary Blog một cách thuận lợi, máy tính phát triển cần đáp ứng các yêu cầu môi trường tối thiểu sau:"
    )

    t2_data = [
        ["Công cụ", "Phiên bản yêu cầu", "Mục đích sử dụng"],
        ["Git", "2.40 trở lên", "Quản lý phiên bản mã nguồn và sao chép kho chứa từ GitHub."],
        ["Docker & Docker Compose", "Docker Desktop mới nhất", "Điều phối các container dịch vụ hạ tầng: PostgreSQL 16, Redis 7, MinIO S3 và Seq."],
        [".NET SDK", "10.0 Preview / .NET 10", "Biên dịch và khởi chạy máy chủ Backend Web API theo kiến trúc Clean Architecture."],
        ["Node.js & npm", "Node 20.x LTS / npm 10.x", "Cài đặt các gói phụ thuộc và chạy máy chủ giao diện người dùng Next.js 16 App Router."]
    ]

    t2 = doc.add_table(rows=len(t2_data), cols=3)
    t2.alignment = WD_TABLE_ALIGNMENT.CENTER
    set_table_borders(t2)
    t2_widths = [Pt(130), Pt(110), Pt(230)]

    for r_idx, row in enumerate(t2.rows):
        for c_idx, cell in enumerate(row.cells):
            cell.width = t2_widths[c_idx]
            set_cell_background(cell, "F9FAFB" if r_idx == 0 or r_idx % 2 == 0 else "FFFFFF")
            set_cell_margins(cell, top=60, bottom=60, left=90, right=90)
            cell.text = t2_data[r_idx][c_idx]
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if c_idx == 1 else WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.line_spacing = 1.15
            p.paragraph_format.space_before = Pt(1)
            p.paragraph_format.space_after = Pt(1)
            p.paragraph_format.first_line_indent = Inches(0)
            r = p.runs[0]
            r.font.name = 'Times New Roman'
            r.font.size = Pt(10)
            if r_idx == 0:
                r.bold = True
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER

    sp_t2 = doc.add_paragraph()
    sp_t2.paragraph_format.space_after = Pt(4)
    sp_t2.paragraph_format.first_line_indent = Inches(0)

    add_p(
        "Lưu ý quan trọng: Trước khi thực hiện các bước khởi chạy dịch vụ, hãy đảm bảo ứng dụng Docker Desktop đã được mở và "
        "trạng thái chuyển sang màu xanh lá cây (Docker Engine running)."
    )

    add_h2("4.2. Các bước cài đặt và khởi chạy chi tiết")

    add_h3("4.2.1. Tải mã nguồn từ GitHub nhánh main chính thức")
    add_p(
        "Mở cửa sổ dòng lệnh (Terminal hoặc PowerShell) tại thư mục làm việc và thực hiện clone mã nguồn dự án để tải về phiên bản mới nhất "
        "đã được tích hợp hoàn chỉnh trên nhánh main:"
    )
    add_code_block([
        "git clone https://github.com/p0Keball/PTUDWNC-2026-Nhom22.git",
        "cd PTUDWNC-2026-Nhom22",
        "git checkout main",
        "git pull origin main"
    ])

    add_h3("4.2.2. Khởi động cơ sở dữ liệu và hạ tầng phụ trợ (Docker Compose)")
    add_p(
        "Hệ thống sử dụng Docker Compose để điều phối toàn bộ cụm dịch vụ phụ trợ bao gồm PostgreSQL 16 (Port 5432), "
        "Redis 7 (Port 6379), MinIO S3 Object Storage (Port 9000 API, Port 9001 Console) và Seq Logging (Port 5341 API, Port 8081 UI). "
        "Tại thư mục gốc của dự án, chạy lệnh:"
    )
    add_code_block([
        "docker compose up -d"
    ])
    add_p(
        "Kiểm tra trạng thái hoạt động: Đảm bảo 4 container đều ở trạng thái Up (healthy): foodblog-postgres, foodblog-redis, foodblog-minio và foodblog-seq."
    )

    add_h3("4.2.3. Khởi động Backend Web API (.NET 10 Minimal APIs)")
    add_p(
        "Khởi chạy máy chủ Backend xử lý nghiệp vụ Clean Architecture bằng lệnh:"
    )
    add_code_block([
        "dotnet run --project src/FoodBlog.API/FoodBlog.API.csproj --launch-profile http"
    ])
    add_p(
        "Quá trình tự động: Máy chủ Backend khi khởi động sẽ tự động áp dụng các bản EF Core Migrations để tạo bảng và nạp sẵn dữ liệu mẫu (Seed Data) phong phú vào cơ sở dữ liệu. "
        "Cửa sổ console xuất hiện thông báo lắng nghe tại địa chỉ: Now listening on: http://localhost:5075. "
        "Kiểm tra tài liệu API tương tác tại địa chỉ: http://localhost:5075/scalar/v1."
    )

    add_h3("4.2.4. Khởi động giao diện người dùng Frontend (Next.js 16 App Router)")
    add_p(
        "Mở một cửa sổ dòng lệnh Terminal mới, di chuyển vào thư mục frontend, cài đặt các gói phụ thuộc và khởi chạy máy chủ phát triển:"
    )
    add_code_block([
        "cd frontend",
        "npm install",
        "npm run dev"
    ])
    add_p(
        "Màn hình Terminal xuất hiện thông báo: Ready in ...ms - Local: http://localhost:3000. "
        "Mở trình duyệt web và truy cập địa chỉ: http://localhost:3000 để trải nghiệm ứng dụng."
    )

    add_h3("4.2.5. Đăng nhập trải nghiệm với các tài khoản mẫu Demo 1-Click")
    add_p(
        "Truy cập vào trang đăng nhập http://localhost:3000/auth/login và sử dụng tiện ích Demo 1-Click hoặc nhập thông tin các tài khoản mẫu sau:"
    )

    t3_data = [
        ["Vai trò", "Địa chỉ Email", "Mật khẩu mặc định", "Tính năng và quyền hạn chính"],
        ["Quản trị viên (Admin)", "admin@foodblog.com", "Admin@123", "Toàn quyền quản trị hệ thống: Quản lý người dùng, phân quyền RBAC, Quản trị Danh mục ẩm thực, Tra cứu Audit Logs (JSON Diff)."],
        ["Tác giả ẩm thực (Author)", "author.giahuy@foodblog.com", "Author@123", "Quyền tác giả: Tạo công thức đa bước (Wizard), Tải ảnh lên MinIO S3, Quản lý vòng đời công thức cá nhân, Chế độ Nấu ăn toàn màn hình."],
        ["Độc giả (User)", "member.khanh@foodblog.com", "User@123", "Quyền độc giả: Tìm kiếm món ăn Full-Text Search không dấu, Xem chi tiết công thức và 6 chỉ số dinh dưỡng, Lưu công thức yêu thích."]
    ]

    t3 = doc.add_table(rows=len(t3_data), cols=4)
    t3.alignment = WD_TABLE_ALIGNMENT.CENTER
    set_table_borders(t3)
    t3_widths = [Pt(110), Pt(120), Pt(90), Pt(150)]

    for r_idx, row in enumerate(t3.rows):
        for c_idx, cell in enumerate(row.cells):
            cell.width = t3_widths[c_idx]
            set_cell_background(cell, "F9FAFB" if r_idx == 0 or r_idx % 2 == 0 else "FFFFFF")
            set_cell_margins(cell, top=60, bottom=60, left=80, right=80)
            cell.text = t3_data[r_idx][c_idx]
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if c_idx in [1, 2] else WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.line_spacing = 1.15
            p.paragraph_format.space_before = Pt(1)
            p.paragraph_format.space_after = Pt(1)
            p.paragraph_format.first_line_indent = Inches(0)
            r = p.runs[0]
            r.font.name = 'Times New Roman'
            r.font.size = Pt(9.5)
            if r_idx == 0:
                r.bold = True
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER

    sp_t3 = doc.add_paragraph()
    sp_t3.paragraph_format.space_after = Pt(4)
    sp_t3.paragraph_format.first_line_indent = Inches(0)

    add_h3("4.2.6. Lệnh chạy bộ kiểm thử tự động toàn diện")
    add_p(
        "Hệ thống tích hợp bộ kiểm thử tự động toàn diện nhằm kiểm chứng tính đúng đắn của logic nghiệp vụ, tính toàn vẹn kiến trúc và độ an toàn bảo mật:"
    )
    add_code_block([
        "dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj --filter \"Category!=RequiresMinIO\""
    ])
    add_p(
        "Kết quả nghiệm thu: Toàn bộ 75+ bài kiểm thử đều đạt trạng thái Passed (100% thành công), không có cảnh báo rủi ro hay lỗi hồi quy nào."
    )

    # -------------------------------------------------------------
    # 7. CHƯƠNG 5: LIÊN KẾT REPOSITORY CHỨA MÃ NGUỒN VÀ CÁC NHÁNH TÍCH HỢP
    # -------------------------------------------------------------
    doc.add_page_break()
    add_h1("CHƯƠNG 5. LIÊN KẾT REPOSITORY CHỨA MÃ NGUỒN VÀ CÁC NHÁNH TÍCH HỢP")

    add_h2("5.1. Kho chứa mã nguồn chính thức của dự án (GitHub)")
    add_p(
        "Kho chứa mã nguồn chính thức của dự án (GitHub): https://github.com/p0Keball/PTUDWNC-2026-Nhom22",
        bold=True, indent=False
    )
    add_p(
        "Nhánh chính thức hoàn chỉnh đã tích hợp của dự án (main): https://github.com/p0Keball/PTUDWNC-2026-Nhom22/tree/main",
        indent=False
    )
    add_p(
        "Nhánh tích hợp tổng thể của dự án: https://github.com/p0Keball/PTUDWNC-2026-Nhom22/tree/QKhanh-restore",
        indent=False
    )

    add_h2("5.2. Nhánh tích hợp và các nhánh làm việc cá nhân của thành viên")
    add_p(
        "Nhánh làm việc của thành viên Trần Gia Huy (Trưởng nhóm): https://github.com/p0Keball/PTUDWNC-2026-Nhom22/tree/GiaHuy",
        indent=False
    )
    add_p(
        "Nhánh làm việc của thành viên Trần Quốc Khánh: https://github.com/p0Keball/PTUDWNC-2026-Nhom22/tree/QKhanh",
        indent=False
    )
    add_p(
        "Nhánh làm việc của thành viên Ngô Văn Trường Long: https://github.com/p0Keball/PTUDWNC-2026-Nhom22/tree/Tlong",
        indent=False
    )
    add_p(
        "Nhánh làm việc của thành viên Trần Đình Việt Huy: https://github.com/p0Keball/PTUDWNC-2026-Nhom22/tree/BrVHuy",
        indent=False
    )

    # Save document
    output_filename = "Nhom22_BaoCao_PTUDWEBNC.docx"
    doc.save(output_filename)
    print(f"Successfully generated {output_filename} with thesis academic format and exact Nhom 14 cover border!")

if __name__ == "__main__":
    create_report()
