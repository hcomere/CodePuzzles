# Executable name definition

set(EXECUTABLE_NAME BattleDev_Nov2016_5_Ronronnements)

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