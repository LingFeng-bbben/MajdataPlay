default:
	@echo "   Create 64-bit Intel (x86_64) library version:"
	@echo "       make x86_64"
	@echo "   Create 64-bit ARM (arm64) library version:"
	@echo "       make arm64"
	@echo "   Create 32-bit Intel (i386) library version:"
	@echo "       make i386"
	@echo "   Create 64-bit (x86_64 and arm64) library version:"
	@echo "       make 64bit"
	@echo "   Create Intel (x86_64 and i386) library version:"
	@echo "       make intel"
	@echo "   Update old library to use @rpath:"
	@echo "       make rpath"
	@echo "   Any add-ons in this folder will be processed too."

BASSLIBS := $(wildcard libbass*.dylib)

x86_64: $(foreach x,$(BASSLIBS),x86_64/$(x))

arm64: $(foreach x,$(BASSLIBS),arm64/$(x))

i386: $(foreach x,$(BASSLIBS),i386/$(x))

64bit: $(foreach x,$(BASSLIBS),64bit/$(x))

intel: $(foreach x,$(BASSLIBS),intel/$(x))

x86_64/%: %
	@mkdir -p x86_64
	-lipo $* -extract x86_64 -output $@

arm64/%: %
	@mkdir -p arm64
	-lipo $* -extract arm64 -output $@

i386/%: %
	@mkdir -p i386
	-lipo $* -extract i386 -output $@

64bit/%: %
	@mkdir -p 64bit
	-lipo $* -extract x86_64 -extract arm64 -output $@

intel/%: %
	@mkdir -p intel
	-lipo $* -extract x86_64 -extract i386 -output $@

rpath:
	@$(foreach x,$(BASSLIBS), \
		if otool -XD $x | grep -q @rpath; then \
			echo "$x already uses @rpath"; \
		else \
			install_name_tool -id @rpath/$x -change @loader_path/libbass.dylib @rpath/libbass.dylib $x && echo "$x updated to use @rpath"; \
		fi;\
	)
