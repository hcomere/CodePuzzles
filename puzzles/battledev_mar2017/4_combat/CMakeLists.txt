# Executable name definition

set(EXECUTABLE_NAME BattleDev_Mar2017_4_Combat)

# Executable creation

add_executable(
    ${EXECUTABLE_NAME}
    main.cpp
)

# Copy data

configure_files(
    ${CMAKE_CURRENT_SOURCE_DIR}/data
    ${CMAKE_CURRENT_BINARY_DIR}/data
)