all: runtimes/linux-x64/native/perf-interface.so runtimes/linux-x86/native/perf-interface.so ../perf-interface.so

runtimes/linux-x64/native/perf-interface.so: perf-interface.c
	mkdir -p runtimes/linux-x64/native
	gcc -m64 --shared -o runtimes/linux-x64/native/perf-interface.so perf-interface.c

runtimes/linux-x86/native/perf-interface.so: perf-interface.c
	mkdir -p runtimes/linux-x86/native
	gcc -m32 --shared -o runtimes/linux-x86/native/perf-interface.so perf-interface.c

../perf-interface.so: perf-interface.c
	# This is the native version to the platform of the developer's machine.
	gcc --shared -o ../perf-interface.so perf-interface.c

clean: 
	rm -r runtimes
	rm ../perf-interface.so
