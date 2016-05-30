.SUFFIXES: .html .tml

.PHONY: default

default: test.html

clean:
	rm -rf *.html

.tml.html:
	mono Tml/bin/Debug/Tml.exe $< > $@

test.html: test.tml
