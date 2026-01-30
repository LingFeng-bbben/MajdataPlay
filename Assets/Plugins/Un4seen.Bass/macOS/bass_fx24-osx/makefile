EXAMPLES = reverse tempo

.PHONY: all clean $(EXAMPLES)

all: $(EXAMPLES)

clean:
	@$(foreach x,$(EXAMPLES),$(MAKE) -C $(x) clean;)

$(EXAMPLES):
	$(MAKE) -C $@

default:
	@echo    Create PowerPC-only library version:
	@echo        make ppc
	@echo    Create 32-bit Intel-only library version:
	@echo        make i386
	@echo    Create 64-bit Intel-only library version:
	@echo        make x86_64
	@echo    Create Universal Intel-only library version:
	@echo        make intel

BASSLIBS := $(wildcard libbass*.dylib)

ppc: $(foreach x,$(BASSLIBS),ppc/$(x))

i386: $(foreach x,$(BASSLIBS),i386/$(x))

x86_64: $(foreach x,$(BASSLIBS),x86_64/$(x))

intel: $(foreach x,$(BASSLIBS),intel/$(x))

ppc/%: %
	@mkdir -p ppc
	-lipo $* -extract_family ppc -output $@

i386/%: %
	@mkdir -p i386
	-lipo $* -extract i386 -output $@

x86_64/%: %
	@mkdir -p x86_64
	-lipo $* -extract x86_64 -output $@

intel/%: %
	@mkdir -p intel
	-lipo $* -extract i386 -extract x86_64 -output $@
